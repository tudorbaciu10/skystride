using OpenTK;
using OpenTK.Input;
using skystride.vendor.collision;
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

        // previous frame position (for collision rollback)
        private Vector3 previousPosition;

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
        private float moveSpeed = 26.0f; // base desired speed, default 6.0f
        private float sprintMultiplier = 2.0f;
        private float jumpSpeed = 6.5f; // initial jump velocity
        private float gravity = -18.0f; // gravity acceleration (m/s^2)
        private float groundY = -10.0f; // flat ground plane at Y=0
        private float eyeHeight = 1.7f; // eye height above ground

        private float groundAccel = 60.0f; // ground acceleration
        private float airAccel = 15.0f; // air acceleration when moving forward/back
        private float sideStrafeAccel = 50.0f; // acceleration when only strafing in air
        private float sideStrafeSpeed = 2.0f; // target wishspeed when side-strafing in air
        private float friction = 6.0f; // ground friction
        private float stopSpeed = 1.0f; // minimum speed considered for friction control

        private Vector3 velocity; // current velocity
        private bool isGrounded = false; // grounded flag

        private float hitboxSize = 1f;

        // turn physics on by default to allow standing/walking
        private bool physicsEnabled = false;

        public Camera(Vector3 _position, float _aspectRatio)
        {
            this.position = _position;
            this.previousPosition = _position;
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

        public AABB Hitbox()
        {
            return new AABB(this.position, new Vector3(this.hitboxSize, 2f, this.hitboxSize));
        }

        public void ResolveCollisions(IEnumerable<AABB> colliders)
        {
            if (colliders == null) return;
            if(physicsEnabled == false) return;

            // Camera half-extents (based on Hitbox())
            float halfX = this.hitboxSize * 0.5f; //0.5f
            float halfY = 1.0f; // since hitbox Y size is2f
            float halfZ = this.hitboxSize * 0.5f;

            Vector3 p = this.position;

            bool groundedThisFrame = false;

            foreach (var c in colliders)
            {
                if (c == null) continue;

                // compute current camera box and collider box using local p
                Vector3 camMin = new Vector3(p.X - halfX, p.Y - halfY, p.Z - halfZ);
                Vector3 camMax = new Vector3(p.X + halfX, p.Y + halfY, p.Z + halfZ);
                Vector3 colMin = c.Min;
                Vector3 colMax = c.Max;

                // quick reject
                if (camMax.X <= colMin.X || camMin.X >= colMax.X ||
                    camMax.Y <= colMin.Y || camMin.Y >= colMax.Y ||
                    camMax.Z <= colMin.Z || camMin.Z >= colMax.Z)
                {
                    continue;
                }

                // horizontal overlap
                bool overlapX = camMax.X > colMin.X && camMin.X < colMax.X;
                bool overlapZ = camMax.Z > colMin.Z && camMin.Z < colMax.Z;

                //1) Vertical resolution (standing on top or hitting head)
                if (overlapX && overlapZ)
                {
                    // coming from above -> land on top
                    float prevBottom = previousPosition.Y - halfY;
                    float prevTop = previousPosition.Y + halfY;

                    if (prevBottom >= colMax.Y && camMin.Y < colMax.Y)
                    {
                        // stand on object
                        p.Y = colMax.Y + halfY;
                        if (velocity.Y < 0f) velocity.Y = 0f;
                        groundedThisFrame = true;

                        camMin.Y = p.Y - halfY;
                        camMax.Y = p.Y + halfY;
                        continue;
                    }

                    // coming from below -> bonk head
                    if (prevTop <= colMin.Y && camMax.Y > colMin.Y)
                    {
                        p.Y = colMin.Y - halfY;
                        if (velocity.Y > 0f) velocity.Y = 0f;
                        camMin.Y = p.Y - halfY;
                        camMax.Y = p.Y + halfY;
                        continue;
                    }
                }

                float penX = Math.Min(camMax.X - colMin.X, colMax.X - camMin.X);
                float penZ = Math.Min(camMax.Z - colMin.Z, colMax.Z - camMin.Z);

                if (penX < penZ)
                {
                    // resolve along X
                    if (previousPosition.X <= colMin.X)
                        p.X = colMin.X - halfX;
                    else if (previousPosition.X >= colMax.X)
                        p.X = colMax.X + halfX;
                    else
                    {
                        // fallback by direction of smallest displacement
                        if ((camMax.X - colMin.X) < (colMax.X - camMin.X))
                            p.X = colMin.X - halfX;
                        else
                            p.X = colMax.X + halfX;
                    }
                }
                else
                {
                    // resolve along Z
                    if (previousPosition.Z <= colMin.Z)
                        p.Z = colMin.Z - halfZ;
                    else if (previousPosition.Z >= colMax.Z)
                        p.Z = colMax.Z + halfZ;
                    else
                    {
                        if ((camMax.Z - colMin.Z) < (colMax.Z - camMin.Z))
                            p.Z = colMin.Z - halfZ;
                        else
                            p.Z = colMax.Z + halfZ;
                    }
                }
            }

            if (groundedThisFrame)
            {
                isGrounded = true;
            }
            this.position = p;
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

            pitch = MathHelper.Clamp(this.pitch, -89f, 89f);

            UpdateVectors();
        }

        public void UpdatePhysics(KeyboardState current, KeyboardState previous, float dt)
        {
            if (dt <= 0f) return;

            // store previous position for collision rollback/resolution
            this.previousPosition = this.position;

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

                float speed = moveSpeed * (current.IsKeyDown(Key.ShiftLeft) ? sprintMultiplier : 1f);

                if (dir.LengthSquared > 0f)
                {
                    dir.NormalizeFast();
                    position += dir * speed * dt;
                }
                return;
            }

            // planar movement basis
            Vector3 forward = front; forward.Y = 0f; if (forward.LengthSquared > 0f) forward.NormalizeFast();
            Vector3 rightVec = right; rightVec.Y = 0f; if (rightVec.LengthSquared > 0f) rightVec.NormalizeFast();

            // desired input direction (wishdir)
            Vector3 wishDir = Vector3.Zero;
            bool fwd = current.IsKeyDown(Key.W);
            bool back = current.IsKeyDown(Key.S);
            bool rightKey = current.IsKeyDown(Key.D);
            bool leftKey = current.IsKeyDown(Key.A);
            if (fwd) wishDir += forward;
            if (back) wishDir -= forward;
            if (rightKey) wishDir += rightVec;
            if (leftKey) wishDir -= rightVec;
            if (wishDir.LengthSquared > 0f) wishDir.NormalizeFast();

            // sprint modifies target speed
            float targetSpeed = moveSpeed * (current.IsKeyDown(Key.ShiftLeft) ? sprintMultiplier : 1f);

            bool jumpPressed = current.IsKeyDown(Key.Space) && !previous.IsKeyDown(Key.Space);
            bool jumpHeld = current.IsKeyDown(Key.Space);

            // ground handling
            if (isGrounded)
            {
                ApplyFriction(ref velocity, dt);

                if (wishDir.LengthSquared > 0f)
                {
                    Accelerate(ref velocity, wishDir, targetSpeed, groundAccel, dt);
                }

                if (jumpPressed || (jumpHeld))
                {
                    // jump impulse upward
                    velocity.Y = jumpSpeed;

                    Vector3 jumpPushDir = forward;
                    if (jumpPushDir.LengthSquared > 0f)
                    {
                        float push = Math.Max(1.0f, targetSpeed * 0.1f); // configurable push
                        velocity.X += jumpPushDir.X * push;
                        velocity.Z += jumpPushDir.Z * push;
                    }

                    isGrounded = false;
                }
            }
            else
            {
                // air control and strafing
                if (wishDir.LengthSquared > 0f)
                {
                    bool onlyStrafe = !fwd && !back && (leftKey ^ rightKey);
                    float accel = onlyStrafe ? sideStrafeAccel : airAccel;
                    float wishSpeed = onlyStrafe ? sideStrafeSpeed : targetSpeed;
                    Accelerate(ref velocity, wishDir, wishSpeed, accel, dt);
                }

                // optional very small air drag to stabilize
                //velocity.X *=1f; velocity.Z *=1f;
            }

            // gravity always
            velocity.Y += gravity * dt;

            // integrate
            Vector3 pos = position;
            pos += velocity * dt;

            // ground plane clamp (fallback if not on top of any collider)
            float minY = groundY + eyeHeight;
            if (pos.Y <= minY)
            {
                pos.Y = minY;
                if (velocity.Y < 0f) velocity.Y = 0f;
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            position = pos;
        }

        private void Accelerate(ref Vector3 vel, Vector3 wishDir, float wishSpeed, float accel, float dt)
        {
            float currentSpeed = Vector3.Dot(new Vector3(vel.X, 0f, vel.Z), wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f) return;
            float accelSpeed = accel * wishSpeed * dt;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;
            vel.X += wishDir.X * accelSpeed;
            vel.Z += wishDir.Z * accelSpeed;
        }

        private void ApplyFriction(ref Vector3 vel, float dt)
        {
            Vector3 lateral = new Vector3(vel.X, 0f, vel.Z);
            float speed = lateral.Length;
            if (speed <= 0.0001f) return;

            float control = speed < stopSpeed ? stopSpeed : speed;
            float drop = control * friction * dt;

            float newSpeed = speed - drop;
            if (newSpeed < 0f) newSpeed = 0f;
            if (newSpeed != speed)
            {
                newSpeed /= speed;
                vel.X *= newSpeed;
                vel.Z *= newSpeed;
            }
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
