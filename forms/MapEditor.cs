using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using skystride.vendor;
using skystride.scenes;

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

        public static void LaunchOrFocus()
        {
            if (IsRunning)
            {
                try
                {
                    _instance.BeginInvoke((Action)(() =>
                    {
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
                _instance = new MapEditor();
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

        // editor camera & scene
        private Camera editorCamera;
        private GlobalScene activeScene;

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

        public MapEditor()
        {
            InitializeComponent();
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
    }
}
