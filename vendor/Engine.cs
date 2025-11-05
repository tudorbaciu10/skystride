using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skystride.objects.templates;
using skystride.scenes;

namespace skystride.vendor
{
    internal class Engine : GameWindow
    {
        // inits
        private KeyboardState currentKeyboardState, previousKeyboardState;
        private MouseState currentMouseState, previousMouseState;

        // main instances
        Camera camera;
        private bool firstCameraMove = true;
        private bool isMouseCentered = false;
        private Vector2 latestMousePosition;

        // init engine window
        public Engine() : base(800, 600, new GraphicsMode(32, 24, 0, 8))
        {
            VSync = VSyncMode.On;
        }

        // on load event
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.LightBlue);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.LineSmooth);

            camera = new Camera(new Vector3(0, 5, 3), Width / (float)Height);

            CursorVisible = false;
            this.isMouseCentered = true;
        }

        // on resize event
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, this.Width, this.Height);

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

            if (currentKeyboardState[Key.Escape])
            {
                CursorVisible = true;
                this.isMouseCentered = false;
            }

            if(currentMouseState.LeftButton == ButtonState.Pressed)
            {
                CursorVisible = false;
                this.isMouseCentered = true;
            }

            if (!CursorVisible && this.isMouseCentered && Focused)
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
                camera.UpdateMouseState(this.currentMouseState);
            }

            this.previousKeyboardState = this.currentKeyboardState;

            camera.UpdateKeyboardState(this.currentKeyboardState, (float)e.Time);
        }

        // on render frame event
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 viewMatrix = camera.GetViewMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref viewMatrix);

            TemplateScene scene = new TemplateScene();
            scene.Render();

            SwapBuffers();
        }
    }
}
