using OpenTK;
using OpenTK.Input;
using skystride.vendor.collision;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace skystride.vendor
{
    internal class Camera
    {
        // transform vectors
        public Vector3 position { get; private set; }
        public Vector3 front { get; private set; } = -Vector3.UnitZ;
        public Vector3 up { get; private set; } = Vector3.UnitY;
        public Vector3 right { get; private set; } = Vector3.UnitX;

        // euler rotations
        private float yaw = -90.0f;
        private float pitch = 0.0f;

        // projection
        private float fov = 60.0f;
        public float Fov { get { return fov; } set { fov = value; } }
        private float aspectRatio;

        // yaw/pitch accessors
        public float YawDegrees { get { return yaw; } }
        public float PitchDegrees { get { return pitch; } }

        public Camera(Vector3 _position, float _aspectRatio)
        {
            this.position = _position;
            this.aspectRatio = _aspectRatio;
            this.UpdateVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(this.position, this.position + this.front, this.up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.fov), this.aspectRatio, 0.1f, 1000f);
        }

        private void UpdateVectors()
        {
            Vector3 _frontState;
            _frontState.X = (float)(System.Math.Cos(MathHelper.DegreesToRadians(this.yaw)) * System.Math.Cos(MathHelper.DegreesToRadians(this.pitch)));
            _frontState.Y = (float)System.Math.Sin(MathHelper.DegreesToRadians(this.pitch));
            _frontState.Z = (float)(System.Math.Sin(MathHelper.DegreesToRadians(this.yaw)) * System.Math.Cos(MathHelper.DegreesToRadians(this.pitch)));

            this.front = Vector3.Normalize(_frontState);
            this.right = Vector3.Normalize(Vector3.Cross(this.front, Vector3.UnitY));
            this.up = Vector3.Normalize(Vector3.Cross(this.right, this.front));
        }

        public void Resize(float _aspect_ratio)
        {
            this.aspectRatio = _aspect_ratio;
        }

        public void SetPosition(Vector3 newPosition)
        {
            this.position = newPosition;
        }

        public void SetRotation(float yaw, float pitch)
        {
            this.yaw = yaw;
            this.pitch = pitch;
        }

        public void SetVectors(Vector3 front, Vector3 up, Vector3 right)
        {
            this.front = front;
            this.up = up;
            this.right = right;
        }

        // crosshair
        public void RenderCrosshair(int screenWidth, int screenHeight)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, screenWidth, screenHeight, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            bool depthEnabled = GL.IsEnabled(EnableCap.DepthTest);
            if (depthEnabled) GL.Disable(EnableCap.DepthTest);

            // draw plus
            GL.Color3(Color.White);
            GL.LineWidth(2f);

            float cx = screenWidth / 2f;
            float cy = screenHeight / 2f;
            float halfLen = 8f; // length from center to end

            GL.Begin(PrimitiveType.Lines);
            // vertical line
            GL.Vertex2(cx, cy - halfLen);
            GL.Vertex2(cx, cy + halfLen);
            // horizontal line
            GL.Vertex2(cx - halfLen, cy);
            GL.Vertex2(cx + halfLen, cy);
            GL.End();

            if (depthEnabled) GL.Enable(EnableCap.DepthTest);

            // Restore matrices
            GL.PopMatrix(); // modelview
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }
    }
}
