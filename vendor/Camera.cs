using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private float sensitivity = 0.2f;

        // projection
        private float fov = 60.0f;
        private float aspectRatio;

        // mouse state
        private bool firstMoveState = true;
        private Vector2 latestMousePosition;

        // physics fields
        private float moveSpeed = 6.0f; // horizontal move speed (m/s)
        private float jumpSpeed = 6.5f; // initial jump velocity
        private float gravity = -18.0f; // gravity acceleration (m/s^2)
        private float groundY = 0.0f; // flat ground plane at Y=0
        private float damping = 8.0f; // air damping for horizontal velocity blending
        private float eyeHeight = 1.7f; // eye height above ground
        private Vector3 velocity; // current velocity
        private bool isGrounded = false; // grounded flag

        private bool physicsEnabled = false; // toggle for physics vs free-fly

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
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.fov), this.aspectRatio,0.1f,1000f);
        }

        // Mouse look
        public void UpdateMouseState(MouseState _currentMouseState)
        {
            Vector2 _mousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);

            if (this.firstMoveState)
            {
                this.latestMousePosition = _mousePosition;
                this.firstMoveState = false;
            }

            float deltaX = _currentMouseState.X - this.latestMousePosition.X;
            float deltaY = _currentMouseState.Y - this.latestMousePosition.Y;
            this.latestMousePosition = _mousePosition;

            this.yaw += deltaX * this.sensitivity;
            this.pitch -= deltaY * this.sensitivity;

            pitch = MathHelper.Clamp(this.pitch, -89f,89f);

            UpdateVectors();
        }

        public void UpdatePhysics(KeyboardState current, KeyboardState previous, float dt)
        {
            if (dt <=0f) return;

            // Free-fly mode (no gravity / physics constraints)
            if (!physicsEnabled)
            {
                Vector3 dir = Vector3.Zero;
                if (current.IsKeyDown(Key.W)) dir += front; // forward
                if (current.IsKeyDown(Key.S)) dir -= front; // backward
                if (current.IsKeyDown(Key.D)) dir += right; // right
                if (current.IsKeyDown(Key.A)) dir -= right; // left
                if (current.IsKeyDown(Key.Space)) dir += Vector3.UnitY; // up
                if (current.IsKeyDown(Key.ControlLeft)) dir -= Vector3.UnitY; // down

                if (current.IsKeyDown(Key.ShiftLeft))
                {
                    this.moveSpeed = 12.0f; // sprint
                } else
                {
                    this.moveSpeed = 6.0f; // normal speed
                }

                if (dir.LengthSquared > 0f)
                {
                    dir.NormalizeFast();
                    position += dir * moveSpeed * dt;
                }
                return; // skip physics section
            }

            // planar movement basis
            Vector3 forward = front; forward.Y =0f; if (forward.LengthSquared >0f) forward.NormalizeFast();
            Vector3 rightVec = right; rightVec.Y =0f; if (rightVec.LengthSquared >0f) rightVec.NormalizeFast();

            // desired direction
            Vector3 wishDir = Vector3.Zero;
            if (current.IsKeyDown(Key.W)) wishDir += forward;
            if (current.IsKeyDown(Key.S)) wishDir -= forward;
            if (current.IsKeyDown(Key.D)) wishDir += rightVec;
            if (current.IsKeyDown(Key.A)) wishDir -= rightVec;
            if (wishDir.LengthSquared >0f) wishDir.NormalizeFast();

            // horizontal velocity smoothing
            Vector3 targetHorizontalVel = wishDir * moveSpeed;
            Vector3 currentHorizontalVel = new Vector3(velocity.X,0f, velocity.Z);
            float t =1f - (float)Math.Exp(-damping * dt);
            Vector3 newHorizontalVel = currentHorizontalVel + (targetHorizontalVel - currentHorizontalVel) * t;
            velocity.X = newHorizontalVel.X;
            velocity.Z = newHorizontalVel.Z;

            // jumping (edge trigger)
            bool jumpPressed = current.IsKeyDown(Key.Space) && !previous.IsKeyDown(Key.Space);
            if (jumpPressed && isGrounded)
            {
                velocity.Y = jumpSpeed;
                isGrounded = false;
            }

            // gravity
            velocity.Y += gravity * dt;

            // integrate
            Vector3 pos = position;
            pos += velocity * dt;

            // ground collision
            float minY = groundY + eyeHeight;
            if (pos.Y <= minY)
            {
                pos.Y = minY;
                if (velocity.Y <0f) velocity.Y =0f;
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            position = pos;
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

        public void AddPosition(Vector3 delta)
        {
            this.position += delta;
        }
    }
}
