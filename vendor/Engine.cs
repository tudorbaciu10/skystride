using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Drawing;
using skystride.objects.templates;
using skystride.scenes;
using skystride.shaders;
using skystride.forms;

namespace skystride.vendor
{
    internal class Engine : GameWindow
    {
        // global input gating
        public static bool InputEnabled = true;
        public static bool BlockShootOnce = false;

        // inits
        private KeyboardState currentKeyboardState, previousKeyboardState;
        private MouseState currentMouseState, previousMouseState;

        // main instances
        Camera camera;
        public Lighting lightning = new Lighting();
        Player player = new Player();
        private bool isMouseCentered = false;

        // shader instances
        private Fog fog;

        // scene instance
        private GlobalScene activeScene;

        // console system
        private GameConsole gameConsole;

        // init engine window
        public Engine() : base(800,600, new GraphicsMode(32,24,0,8))
        {
            VSync = VSyncMode.On;
            Title = "Skystride Engine";
            //WindowState = WindowState.Maximized;

            X = (DisplayDevice.Default.Width - Width) /2;
            Y = (DisplayDevice.Default.Height - Height) /2;
        }

        // on load event
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.DarkBlue);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.LineSmooth);

            camera = new Camera(new Vector3(0,5,3), Width / (float)Height);

            gameConsole = new GameConsole(camera, player, this);

            fog = new Fog(Color.DarkBlue, FogMode.Exp2,0.005f,30f,250f);

            activeScene = new TemplateScene();
            //activeScene = new ArcticScene();
            //activeScene = new ForestScene();

            CursorVisible = false;
            this.isMouseCentered = true;

            // initialize input flags
            InputEnabled = true;
            BlockShootOnce = false;
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            activeScene?.Dispose();
        }

        // on resize event
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0,0, this.Width, this.Height);

            camera.Resize(Width / (float)Height);

            // Update projection on resize
            var projection = camera.GetProjectionMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        // on update frame event
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            this.currentKeyboardState = Keyboard.GetState();
            this.currentMouseState = Mouse.GetState();

            // Toggle console with Tilde / Grave accent key
            if (currentKeyboardState.IsKeyDown(Key.Tilde) && !previousKeyboardState.IsKeyDown(Key.Tilde))
            {
                gameConsole.Toggle();
            }

            if (currentKeyboardState.IsKeyDown(Key.F3) && !previousKeyboardState.IsKeyDown(Key.F3))
            {
                MapEditor.LaunchOrFocus(player);
            }

            if (Focused)
            {
                if (!gameConsole.IsOpen)
                {
                    if (currentKeyboardState[Key.Escape])
                    {
                        CursorVisible = true;
                        this.isMouseCentered = false;
                    }

                    if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                    {
                        CursorVisible = false;
                        this.isMouseCentered = true;
                    }

                    InputEnabled = !skystride.forms.MapEditor.EditorHasFocus;
                }
                else
                {
                    InputEnabled = false;
                }
            }
            else
            {
                CursorVisible = true;
                this.isMouseCentered = false;
                InputEnabled = false;
            }

            if (!CursorVisible && this.isMouseCentered && Focused && !gameConsole.IsOpen)
            {
                Mouse.SetPosition(X + Width /2f, Y + Height /2f);
                player.UpdateMouseState(this.currentMouseState);
            }

            if (!gameConsole.IsOpen && InputEnabled)
            {
                player.Update(currentKeyboardState, previousKeyboardState, (float)e.Time);
            }

            // Sync camera with player
            player.UpdateCamera(camera);

            // Scene update always (can be paused if desired later)
            activeScene?.Update((float)e.Time, player, camera, currentKeyboardState, previousKeyboardState, currentMouseState, previousMouseState);

            // Update console after capturing key states
            gameConsole.Update(currentKeyboardState);

            this.previousKeyboardState = this.currentKeyboardState;
            this.previousMouseState = this.currentMouseState;
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            if (!Focused)
            {
                CursorVisible = true;
                this.isMouseCentered = false;

                BlockShootOnce = true;
                InputEnabled = false;
            }
            else
            {
                if (!gameConsole.IsOpen)
                {
                    CursorVisible = false;
                    this.isMouseCentered = true;
                    BlockShootOnce = true; // prevent accidental shot on focus regain
                    InputEnabled = true;
                }
            }
        }

        // on render frame event
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 viewMatrix = this.camera.GetViewMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref viewMatrix);

            activeScene?.Render();
            lightning.Render();

            this.fog.Render();

            // render first-person weapon before crosshair/UI
            player.RenderWeapon(camera);

            GL.Disable(EnableCap.Lighting);
            // crosshair
            camera.RenderCrosshair(Width, Height);

            TextRenderer.RenderText($"x = {player.position.X}, y = {player.position.Y}, z = {player.position.Z}",16,24, Color.White, Width, Height);

            // player info moved to left bottom
            TextRenderer.RenderText($"{player.GetHealth()}+",32, Height -64, Color.DarkOrange, Width, Height,32f);

            if (player.HasAttachedWeapon())
            {
                string ammoText = $"{player.GetAmmo()}";
                float fontSize = 32f;
                float textWidth = ammoText.Length * (fontSize * 0.6f);
                TextRenderer.RenderText(ammoText, Width - (int)textWidth - 32, Height - 64, Color.DarkOrange, Width, Height, fontSize);
            }

            // Render console overlay 
            gameConsole.Render(Width, Height);

            player.RenderHUD(Width, Height);

            if (lightning.enabled) GL.Enable(EnableCap.Lighting);

            SwapBuffers();
        }

        // Scene change API used by console
        internal bool ChangeScene(string mapName)
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
                    return false; // unsupported map
            }

            activeScene?.Dispose();
            activeScene = newScene;

            player.SetPosition(new Vector3(0f, 5f, 3f));
            player.UpdateCamera(camera);
            MapEditor.UpdateScene(mapName);
            return true;
        }
    }
}