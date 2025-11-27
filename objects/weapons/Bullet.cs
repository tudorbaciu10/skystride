using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using skystride.scenes;
using skystride.vendor.collision;
using System.Drawing;
using System.Collections.Generic;

namespace skystride.objects.weapons
{
    internal class Bullet : ISceneEntity
    {
        public Vector3 Position { get; private set; }
        public Vector3 GetPosition() { return Position; }
        public void SetPosition(Vector3 pos) { Position = pos; }
        public Vector3 GetSize() { return Vector3.Zero; }
        public void SetSize(Vector3 size) { }
        public Vector3 Direction { get; private set; }
        public float Speed { get; private set; }
        public float Lifetime { get; private set; }
        public bool IsDead { get; private set; }

        private float _timeAlive;

        public float Damage { get; private set; } = 25f;

        public Bullet(Vector3 position, Vector3 direction, float speed, float lifetime)
        {
            Position = position;
            Direction = direction;
            Speed = speed;
            Lifetime = lifetime;
            IsDead = false;
            _timeAlive = 0f;
        }

        public void Update(float dt, IEnumerable<AABB> colliders)
        {
            if (IsDead) return;

            _timeAlive += dt;
            if (_timeAlive >= Lifetime)
            {
                IsDead = true;
                return;
            }

            float distance = Speed * dt;
            Vector3 nextPosition = Position + Direction * distance;

            // check collisions
            if (colliders != null)
            {
                foreach (var box in colliders)
                {
                    if (RayIntersectsAABB(Position, Direction, box, distance))
                    {
                        // Check if we hit an NPC
                        if (box.Owner is skystride.vendor.NPC npc)
                        {
                            npc.TakeDamage(Damage);
                        }

                        IsDead = true;
                        return;
                    }
                }
            }

            Position = nextPosition;
        }

        private bool RayIntersectsAABB(Vector3 origin, Vector3 direction, AABB box, float maxDistance)
        {
            Vector3 dirInv = new Vector3(1.0f / direction.X, 1.0f / direction.Y, 1.0f / direction.Z);

            float t1 = (box.Min.X - origin.X) * dirInv.X;
            float t2 = (box.Max.X - origin.X) * dirInv.X;
            float t3 = (box.Min.Y - origin.Y) * dirInv.Y;
            float t4 = (box.Max.Y - origin.Y) * dirInv.Y;
            float t5 = (box.Min.Z - origin.Z) * dirInv.Z;
            float t6 = (box.Max.Z - origin.Z) * dirInv.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but whole AABB is behind us
            if (tmax < 0)
            {
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax)
            {
                return false;
            }

            // if tmin > maxDistance, intersection is too far
            if (tmin > maxDistance)
            {
                return false;
            }

            return true;
        }

        public void Render()
        {
            if (IsDead) return;

            GL.PushMatrix();
            GL.Translate(Position);
            
            // simple visualization: a small yellow cube or point
            GL.Color3(System.Drawing.Color.Yellow);
            GL.PointSize(5.0f);
            
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(0, 0, 0);
            GL.End();

            // or a small line to show direction
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(Direction * 0.5f);
            GL.End();

            GL.PopMatrix();
        }
    }
}
