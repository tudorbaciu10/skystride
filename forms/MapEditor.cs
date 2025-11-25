using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using skystride.vendor;
using skystride.scenes;
using skystride.objects.templates;

namespace skystride.forms
{
    public partial class MapEditor : Form
    {
        // static host
        private static System.Threading.Thread _uiThread;
        private static MapEditor _instance;
        public static volatile bool EditorHasFocus;

        public static bool IsRunning
        {
            get { return _instance != null && !_instance.IsDisposed; }
        }

        internal static void LaunchOrFocus(Player playerRef)
        {
            if (IsRunning)
            {
                try
                {
                    _instance.BeginInvoke((Action)(() =>
                    {
                        _instance.trackedPlayer = playerRef;
                        if (_instance.WindowState == FormWindowState.Minimized)
                            _instance.WindowState = FormWindowState.Normal;
                        _instance.BringToFront();
                        _instance.Activate();
                    }));
                }
                catch { }
                return;
            }

            _uiThread = new System.Threading.Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                _instance = new MapEditor(playerRef);
                _instance.Activated += (s, e) => EditorHasFocus = true;
                _instance.Deactivate += (s, e) => EditorHasFocus = false;
                _instance.FormClosed += (s, e) => EditorHasFocus = false;
                _instance.FormClosed += (s, e) =>
                {
                    _instance = null;
                    try { Application.ExitThread(); } catch { }
                };
                Application.Run(_instance);
            });
            _uiThread.IsBackground = true;
            _uiThread.SetApartmentState(System.Threading.ApartmentState.STA);
            _uiThread.Start();
        }

        internal static void UpdateScene(string mapName)
        {
            if (IsRunning)
            {
                try
                {
                    _instance.BeginInvoke((Action)(() =>
                    {
                        _instance.LoadScene(mapName);
                    }));
                }
                catch { }
            }
        }

        // editor camera & scene
        private Camera editorCamera;
        private GlobalScene activeScene;
        private Player trackedPlayer;
        private Sphere playerMarker;

        // render/update
        private Timer renderTimer;
        private Stopwatch stopwatch;
        private long lastElapsedMs;

        // input state
        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private bool isMouseLook;
        private Point lastMousePos;
        private float yaw = -90.0f;
        private float pitch = 0.0f;
        private const float mouseSensitivity = 0.2f; // similar to player
        private const float moveSpeed = 8.0f;
        
        // UI Controls
        private Panel uiPanel;
        private ListBox lstEntities;
        private NumericUpDown numPosX, numPosY, numPosZ;
        private Button btnAddCube, btnAddSphere, btnAddPlane, btnDelete;
        private Label lblPos;
        private ISceneEntity selectedEntity;
        private bool ignoreEvents = false;

        public MapEditor()
        {
            InitializeComponent();
        }

        internal MapEditor(Player player)
            : this()
        {
            this.trackedPlayer = player;
        }

        private void glControlMapEditor_Load(object sender, EventArgs e)
        {
            glControlMapEditor.MakeCurrent();

            // GL init
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.Lighting);

            // camera init
            float aspect = Math.Max(1, glControlMapEditor.Width) / (float)Math.Max(1, glControlMapEditor.Height);
            editorCamera = new Camera(new Vector3(0f, 5f, 3f), aspect);
            yaw = editorCamera.YawDegrees;
            pitch = editorCamera.PitchDegrees;

            // load same environment as engine
            activeScene = new ForestScene();

            // player marker (white sphere)
            playerMarker = new Sphere(new Vector3(0f, 0.5f, 0f), 0.5f, 1f, Color.White);

            // hook control events for input/render
            glControlMapEditor.Resize += GlControlMapEditor_Resize;
            glControlMapEditor.Paint += GlControlMapEditor_Paint;
            glControlMapEditor.KeyDown += GlControlMapEditor_KeyDown;
            glControlMapEditor.KeyUp += GlControlMapEditor_KeyUp;
            glControlMapEditor.MouseDown += GlControlMapEditor_MouseDown;
            glControlMapEditor.MouseUp += GlControlMapEditor_MouseUp;
            glControlMapEditor.MouseMove += GlControlMapEditor_MouseMove;
            glControlMapEditor.Disposed += GlControlMapEditor_Disposed;
            glControlMapEditor.Leave += GlControlMapEditor_Leave;
            glControlMapEditor.LostFocus += GlControlMapEditor_LostFocus;
            glControlMapEditor.GotFocus += (s2, e2) => EditorHasFocus = true;
            glControlMapEditor.LostFocus += (s2, e2) => EditorHasFocus = false;

            // form-level focus events
            this.Deactivate += MapEditor_Deactivate;
            this.FormClosed += MapEditor_FormClosed;

            glControlMapEditor.TabStop = true; // allow focus by click/tab

            // start render loop
            stopwatch = Stopwatch.StartNew();
            lastElapsedMs = stopwatch.ElapsedMilliseconds;
            renderTimer = new Timer { Interval = 16 }; // ~60 FPS
            renderTimer.Tick += (s, args) =>
            {
                if (!Visible || WindowState == FormWindowState.Minimized) return;
                glControlMapEditor.Invalidate();
            };
            renderTimer.Start();
            renderTimer.Start();
            InitializeEditorUI();
        }

        private void GlControlMapEditor_Disposed(object sender, EventArgs e)
        {
            try { activeScene?.Dispose(); } catch { }
            renderTimer?.Stop();
            renderTimer?.Dispose();
            ExitMouseLook();
        }

        private void MapEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            ExitMouseLook();
            EditorHasFocus = false;
        }

        private void MapEditor_Deactivate(object sender, EventArgs e)
        {
            ExitMouseLook();
            pressedKeys.Clear();
            EditorHasFocus = false;
        }

        private void GlControlMapEditor_Resize(object sender, EventArgs e)
        {
            if (glControlMapEditor.ClientSize.Width <= 0 || glControlMapEditor.ClientSize.Height <= 0) return;
            glControlMapEditor.MakeCurrent();
            GL.Viewport(0, 0, glControlMapEditor.ClientSize.Width, glControlMapEditor.ClientSize.Height);
            float aspect = glControlMapEditor.ClientSize.Width / (float)glControlMapEditor.ClientSize.Height;
            editorCamera?.Resize(aspect);
        }

        private void GlControlMapEditor_Paint(object sender, PaintEventArgs e)
        {
            glControlMapEditor.MakeCurrent();

            // compute delta time
            long now = stopwatch?.ElapsedMilliseconds ?? 0;
            float dt = (now - lastElapsedMs) / 1000f;
            if (dt < 0f || dt > 1f) dt = 0f; // clamp if debugger pauses
            lastElapsedMs = now;

            if (glControlMapEditor.Focused)
            {
                UpdateCameraMovement(dt);
            }

            activeScene?.Update(dt, null, editorCamera,
                default(OpenTK.Input.KeyboardState), default(OpenTK.Input.KeyboardState),
                default(OpenTK.Input.MouseState), default(OpenTK.Input.MouseState));

            // render
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // set matrices
            Matrix4 proj = editorCamera.GetProjectionMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref proj);
            Matrix4 view = editorCamera.GetViewMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref view);

            activeScene?.Render();

            // draw tracked player marker
            if (trackedPlayer != null && playerMarker != null)
            {
                // mirror player's current position
                playerMarker.SetPosition(trackedPlayer.position);
                playerMarker.Render();
            }

            glControlMapEditor.SwapBuffers();
        }

        private void GlControlMapEditor_LostFocus(object sender, EventArgs e)
        {
            ExitMouseLook();
            pressedKeys.Clear();
            EditorHasFocus = false;
        }

        private void GlControlMapEditor_Leave(object sender, EventArgs e)
        {
            ExitMouseLook();
            pressedKeys.Clear();
            EditorHasFocus = false;
        }

        private void UpdateCameraMovement(float dt)
        {
            if (editorCamera == null || dt <= 0f) return;

            // movement
            Vector3 move = Vector3.Zero;
            if (pressedKeys.Contains(Keys.W)) move += GetFlatForward();
            if (pressedKeys.Contains(Keys.S)) move -= GetFlatForward();
            if (pressedKeys.Contains(Keys.D)) move += GetFlatRight();
            if (pressedKeys.Contains(Keys.A)) move -= GetFlatRight();
            if (pressedKeys.Contains(Keys.Space)) move += Vector3.UnitY;
            if (pressedKeys.Contains(Keys.ControlKey)) move -= Vector3.UnitY;

            if (move.LengthSquared > 0)
            {
                move.NormalizeFast();
                var pos = editorCamera.position;
                editorCamera.SetPosition(pos + move * moveSpeed * dt);
            }

            // apply yaw/pitch -> recompute camera vectors
            Vector3 front;
            front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch)));
            front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch)));
            front.NormalizeFast();
            Vector3 right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            Vector3 up = Vector3.Normalize(Vector3.Cross(right, front));
            editorCamera.SetRotation(yaw, pitch);
            editorCamera.SetVectors(front, up, right);
        }

        private Vector3 GetFlatForward()
        {
            // build forward from yaw/pitch but flatten Y
            Vector3 f;
            f.X = (float)(Math.Cos(MathHelper.DegreesToRadians(yaw)));
            f.Y = 0f;
            f.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(yaw)));
            if (f.LengthSquared > 0) f.NormalizeFast();
            return f;
        }

        private Vector3 GetFlatRight()
        {
            Vector3 f = GetFlatForward();
            Vector3 r = Vector3.Normalize(Vector3.Cross(f, Vector3.UnitY));
            return r;
        }

        private void GlControlMapEditor_KeyDown(object sender, KeyEventArgs e)
        {
            pressedKeys.Add(e.KeyCode);
            if (e.KeyCode == Keys.Escape)
            {
                ExitMouseLook();
            }
        }

        private void GlControlMapEditor_KeyUp(object sender, KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
        }

        private void GlControlMapEditor_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!glControlMapEditor.Focused) glControlMapEditor.Focus();
            if (e.Button == MouseButtons.Right)
            {
                isMouseLook = true;
                lastMousePos = e.Location;
                Cursor.Hide();
            }
        }

        private void GlControlMapEditor_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isMouseLook = false;
                Cursor.Show();
            }
        }

        private void GlControlMapEditor_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!isMouseLook) return;
            var dx = e.X - lastMousePos.X;
            var dy = e.Y - lastMousePos.Y;
            lastMousePos = e.Location;

            yaw += dx * mouseSensitivity;
            pitch -= dy * mouseSensitivity;
            pitch = MathHelper.Clamp(pitch, -89f, 89f);
        }

        private void ExitMouseLook()
        {
            if (isMouseLook)
            {
                isMouseLook = false;
            }
            try { Cursor.Show(); } catch { }
        }


        private void LoadScene(string mapName)
        {
            string key = (mapName ?? string.Empty).Trim().ToLowerInvariant();
            GlobalScene newScene = null;
            switch (key)
            {
                case "forest":
                    newScene = new ForestScene();
                    break;
                case "arctic":
                    newScene = new ArcticScene();
                    break;
                default:
                    return;
            }

            activeScene?.Dispose();
            activeScene = newScene;
            RefreshEntityList();
        }
        private void InitializeEditorUI()
        {
            glControlMapEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            uiPanel = new Panel();
            uiPanel.Parent = this;
            uiPanel.Location = new Point(glControlMapEditor.Width + 10, 10);
            uiPanel.Size = new Size(this.ClientSize.Width - glControlMapEditor.Width - 20, this.ClientSize.Height - 20);
            uiPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;

            int y = 0;

            // ListBox
            lstEntities = new ListBox();
            lstEntities.Parent = uiPanel;
            lstEntities.Location = new Point(0, y);
            lstEntities.Size = new Size(uiPanel.Width, 200);
            lstEntities.SelectedIndexChanged += LstEntities_SelectedIndexChanged;
            y += 210;

            // Position Controls
            lblPos = new Label();
            lblPos.Parent = uiPanel;
            lblPos.Text = "Position (X, Y, Z):";
            lblPos.Location = new Point(0, y);
            lblPos.AutoSize = true;
            y += 25;

            numPosX = CreateNumeric(uiPanel, 0, y);
            numPosY = CreateNumeric(uiPanel, 70, y);
            numPosZ = CreateNumeric(uiPanel, 140, y);
            y += 30;

            // Add Buttons
            btnAddCube = CreateButton(uiPanel, "Add Cube", 0, y, (s, e) => AddEntity(new Cube()));
            y += 30;
            btnAddSphere = CreateButton(uiPanel, "Add Sphere", 0, y, (s, e) => AddEntity(new Sphere()));
            y += 30;
            btnAddPlane = CreateButton(uiPanel, "Add Plane", 0, y, (s, e) => AddEntity(new Plane()));
            y += 30;
            btnDelete = CreateButton(uiPanel, "Delete Selected", 0, y, BtnDelete_Click);

            RefreshEntityList();
        }

        private NumericUpDown CreateNumeric(Control parent, int x, int y)
        {
            var num = new NumericUpDown();
            num.Parent = parent;
            num.Location = new Point(x, y);
            num.Width = 60;
            num.DecimalPlaces = 2;
            num.Minimum = -10000;
            num.Maximum = 10000;
            num.ValueChanged += NumPos_ValueChanged;
            return num;
        }

        private Button CreateButton(Control parent, string text, int x, int y, EventHandler onClick)
        {
            var btn = new Button();
            btn.Parent = parent;
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Width = parent.Width;
            btn.Click += onClick;
            return btn;
        }

        private void RefreshEntityList()
        {
            lstEntities.Items.Clear();
            if (activeScene != null)
            {
                var entities = activeScene.GetEntities();
                for (int i = 0; i < entities.Count; i++)
                {
                    lstEntities.Items.Add($"{i}: {entities[i].GetType().Name}");
                }
            }
        }

        private void LstEntities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstEntities.SelectedIndex >= 0 && activeScene != null)
            {
                var entities = activeScene.GetEntities();
                if (lstEntities.SelectedIndex < entities.Count)
                {
                    selectedEntity = entities[lstEntities.SelectedIndex];
                    UpdatePositionControls();
                }
            }
            else
            {
                selectedEntity = null;
            }
        }

        private void UpdatePositionControls()
        {
            if (selectedEntity == null) return;
            ignoreEvents = true;
            var pos = selectedEntity.GetPosition();
            numPosX.Value = (decimal)pos.X;
            numPosY.Value = (decimal)pos.Y;
            numPosZ.Value = (decimal)pos.Z;
            ignoreEvents = false;
        }

        private void NumPos_ValueChanged(object sender, EventArgs e)
        {
            if (ignoreEvents || selectedEntity == null) return;
            var newPos = new Vector3((float)numPosX.Value, (float)numPosY.Value, (float)numPosZ.Value);
            selectedEntity.SetPosition(newPos);
        }

        private void AddEntity(ISceneEntity entity)
        {
            if (activeScene != null)
            {
                activeScene.AddEntity(entity);
                RefreshEntityList();
                lstEntities.SelectedIndex = lstEntities.Items.Count - 1;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedEntity != null && activeScene != null)
            {
                activeScene.RemoveEntity(selectedEntity);
                selectedEntity = null;
                RefreshEntityList();
            }
        }
    }
}
