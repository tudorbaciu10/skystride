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
using skystride.objects;

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
            RefreshEntityList();

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

            // Render Gizmo
            if (selectedEntity != null)
            {
                gizmo.Render(selectedEntity.GetPosition());
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

        // gizmo
        private EditorGizmo gizmo = new EditorGizmo();
        private bool isDraggingGizmo = false;
        private GizmoAxis dragAxis = GizmoAxis.None;
        private Vector3 dragStartPos;
        private Vector3 dragPlaneNormal;
        private Vector3 dragPlanePoint;
        private Vector3 dragStartHitPoint;

        private void BtnAddCube_Click(object sender, EventArgs e)
        {
            AddEntity(new Cube());
        }

        private void BtnAddSphere_Click(object sender, EventArgs e)
        {
            AddEntity(new Sphere());
        }

        private void BtnAddPlane_Click(object sender, EventArgs e)
        {
            AddEntity(new Plane());
        }

        private Ray GetPickRay(int mouseX, int mouseY)
        {
            if (glControlMapEditor.Width == 0 || glControlMapEditor.Height == 0) return new Ray();

            float x = (2.0f * mouseX) / glControlMapEditor.Width - 1.0f;
            float y = 1.0f - (2.0f * mouseY) / glControlMapEditor.Height;
            Vector4 rayClip = new Vector4(x, y, -1.0f, 1.0f);

            Matrix4 proj = editorCamera.GetProjectionMatrix();
            Matrix4 view = editorCamera.GetViewMatrix();

            Matrix4 invProj = Matrix4.Invert(proj);
            Vector4 rayEye = Vector4.Transform(rayClip, invProj);
            rayEye = new Vector4(rayEye.X, rayEye.Y, -1.0f, 0.0f);

            Matrix4 invView = Matrix4.Invert(view);
            Vector4 rayWorld = Vector4.Transform(rayEye, invView);
            Vector3 rayDir = new Vector3(rayWorld.X, rayWorld.Y, rayWorld.Z);
            if (rayDir.LengthSquared > 0) rayDir.Normalize();

            return new Ray(editorCamera.position, rayDir);
        }

        private Vector3? IntersectPlane(Ray ray, Vector3 planeNormal, Vector3 planePoint)
        {
            float denom = Vector3.Dot(planeNormal, ray.Direction);
            if (Math.Abs(denom) > 0.0001f)
            {
                float t = Vector3.Dot(planePoint - ray.Origin, planeNormal) / denom;
                if (t >= 0) return ray.Origin + ray.Direction * t;
            }
            return null;
        }

        private void GlControlMapEditor_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!glControlMapEditor.Focused) glControlMapEditor.Focus();
            
            if (e.Button == MouseButtons.Left)
            {
                Ray ray = GetPickRay(e.X, e.Y);
                
                if (selectedEntity != null)
                {
                    GizmoAxis hit = gizmo.CheckIntersection(ray, selectedEntity.GetPosition());
                    if (hit != GizmoAxis.None)
                    {
                        isDraggingGizmo = true;
                        dragAxis = hit;
                        gizmo.SetSelectedAxis(hit);
                        dragStartPos = selectedEntity.GetPosition();
                        
                        // Determine drag plane
                        Vector3 viewDir = editorCamera.front;
                        dragPlanePoint = dragStartPos;

                        if (dragAxis == GizmoAxis.X)
                        {
                            dragPlaneNormal = Math.Abs(viewDir.Y) > Math.Abs(viewDir.Z) ? Vector3.UnitY : Vector3.UnitZ;
                        }
                        else if (dragAxis == GizmoAxis.Y)
                        {
                            dragPlaneNormal = Math.Abs(viewDir.X) > Math.Abs(viewDir.Z) ? Vector3.UnitX : Vector3.UnitZ;
                        }
                        else // Z
                        {
                            dragPlaneNormal = Math.Abs(viewDir.X) > Math.Abs(viewDir.Y) ? Vector3.UnitX : Vector3.UnitY;
                        }

                        var hitPoint = IntersectPlane(ray, dragPlaneNormal, dragPlanePoint);
                        if (hitPoint.HasValue)
                        {
                            dragStartHitPoint = hitPoint.Value;
                        }
                        else
                        {
                            isDraggingGizmo = false; // Should not happen if we hit the gizmo
                        }
                        return; // Consume event
                    }
                }

                ISceneEntity closest = GetClosestEntity(ray);
                if (closest != null)
                {
                    selectedEntity = closest;
                    // Sync ListBox
                    if (activeScene != null)
                    {
                        var list = activeScene.GetEntities();
                        int idx = list.IndexOf(closest);
                        if (idx >= 0 && idx < lstEntities.Items.Count)
                        {
                            lstEntities.SelectedIndex = idx;
                        }
                    }
                    UpdatePositionControls();
                }
                else
                {
                    // Deselect if clicked on empty space
                    selectedEntity = null;
                    lstEntities.SelectedIndex = -1;
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                isMouseLook = true;
                lastMousePos = e.Location;
                Cursor.Hide();
            }
        }

        private ISceneEntity GetClosestEntity(Ray ray)
        {
            if (activeScene == null) return null;

            ISceneEntity closestEntity = null;
            float closestDist = float.MaxValue;

            var entities = activeScene.GetEntities();
            foreach (var entity in entities)
            {
                float dist = float.MaxValue;
                bool hit = false;

                if (entity is Cube cube)
                {
                    float s = cube.GetSize();
                    Vector3 pos = cube.GetPosition();
                    Vector3 min = pos - new Vector3(s / 2f);
                    Vector3 max = pos + new Vector3(s / 2f);
                    hit = RayIntersectsAABB(ray, min, max, out dist);
                }
                else if (entity is Sphere sphere)
                {
                    hit = RayIntersectsSphere(ray, sphere.GetPosition(), sphere.GetRadius(), out dist);
                }
                else if (entity is Plane plane)
                {
                    // Treat plane as thin AABB
                    Vector3 size = plane.GetSize();
                    Vector3 pos = plane.GetPosition();
                   
                    float sx = Math.Max(size.X, 0.1f);
                    float sy = Math.Max(size.Y, 0.1f); // planes usually flat on Y or Z depending on rotation, but size is dimensions
                    float sz = Math.Max(size.Z, 0.1f);
                    
                  
                    float maxDim = Math.Max(sx, Math.Max(sy, sz));
                    Vector3 min = pos - new Vector3(maxDim / 2f);
                    Vector3 max = pos + new Vector3(maxDim / 2f);
                    hit = RayIntersectsAABB(ray, min, max, out dist);
                }
                else if (entity.GetType().Name == "ModelEntity") // Reflection or check type if public
                {
                    Vector3 pos = entity.GetPosition();
                    Vector3 min = pos - new Vector3(1f);
                    Vector3 max = pos + new Vector3(1f);
                    hit = RayIntersectsAABB(ray, min, max, out dist);
                }

                if (hit && dist < closestDist)
                {
                    closestDist = dist;
                    closestEntity = entity;
                }
            }

            return closestEntity;
        }

        private bool RayIntersectsAABB(Ray ray, Vector3 min, Vector3 max, out float distance)
        {
            distance = 0f;
            float tmin = (min.X - ray.Origin.X) / ray.Direction.X;
            float tmax = (max.X - ray.Origin.X) / ray.Direction.X;

            if (tmin > tmax) { float temp = tmin; tmin = tmax; tmax = temp; }

            float tymin = (min.Y - ray.Origin.Y) / ray.Direction.Y;
            float tymax = (max.Y - ray.Origin.Y) / ray.Direction.Y;

            if (tymin > tymax) { float temp = tymin; tymin = tymax; tymax = temp; }

            if ((tmin > tymax) || (tymin > tmax)) return false;

            if (tymin > tmin) tmin = tymin;
            if (tymax < tmax) tmax = tymax;

            float tzmin = (min.Z - ray.Origin.Z) / ray.Direction.Z;
            float tzmax = (max.Z - ray.Origin.Z) / ray.Direction.Z;

            if (tzmin > tzmax) { float temp = tzmin; tzmin = tzmax; tzmax = temp; }

            if ((tmin > tzmax) || (tzmin > tmax)) return false;

            if (tzmin > tmin) tmin = tzmin;
            if (tzmax < tmax) tmax = tzmax;

            if (tmax < 0) return false; // Box is behind ray

            distance = tmin >= 0 ? tmin : tmax;
            return true;
        }

        private bool RayIntersectsSphere(Ray ray, Vector3 center, float radius, out float distance)
        {
            distance = 0f;
            Vector3 m = ray.Origin - center;
            float b = Vector3.Dot(m, ray.Direction);
            float c = Vector3.Dot(m, m) - radius * radius;

            if (c > 0.0f && b > 0.0f) return false;

            float discr = b * b - c;
            if (discr < 0.0f) return false;

            distance = -b - (float)Math.Sqrt(discr);
            if (distance < 0.0f) distance = 0.0f;
            return true;
        }

        private void GlControlMapEditor_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isDraggingGizmo && e.Button == MouseButtons.Left)
            {
                isDraggingGizmo = false;
                dragAxis = GizmoAxis.None;
                gizmo.SetSelectedAxis(GizmoAxis.None);
            }

            if (e.Button == MouseButtons.Right)
            {
                isMouseLook = false;
                Cursor.Show();
            }
        }

        private void GlControlMapEditor_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isDraggingGizmo && selectedEntity != null)
            {
                Ray ray = GetPickRay(e.X, e.Y);
                var hitPoint = IntersectPlane(ray, dragPlaneNormal, dragPlanePoint);
                if (hitPoint.HasValue)
                {
                    Vector3 currentHitPoint = hitPoint.Value;
                    Vector3 delta = currentHitPoint - dragStartHitPoint;
                    Vector3 newPos = dragStartPos;

                    if (dragAxis == GizmoAxis.X) newPos.X += delta.X;
                    if (dragAxis == GizmoAxis.Y) newPos.Y += delta.Y;
                    if (dragAxis == GizmoAxis.Z) newPos.Z += delta.Z;

                    selectedEntity.SetPosition(newPos);
                    UpdatePositionControls();
                }
                return;
            }

            if (!isMouseLook)
            {
                // Hover effect
                if (selectedEntity != null && !isDraggingGizmo)
                {
                    Ray ray = GetPickRay(e.X, e.Y);
                    GizmoAxis hit = gizmo.CheckIntersection(ray, selectedEntity.GetPosition());
                    gizmo.SetSelectedAxis(hit);
                }
                return;
            }

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
