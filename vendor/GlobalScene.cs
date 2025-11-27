using OpenTK;
using OpenTK.Input;
using skystride.objects;
using skystride.objects.templates;
using skystride.vendor;
using skystride.vendor.collision;
using skystride.objects.weapons;
using System;
using System.Collections.Generic;

namespace skystride.scenes
{
    internal class GlobalScene : IDisposable
    {
        // Unified list for all entities
        protected readonly List<ISceneEntity> Entities = new List<ISceneEntity>();
        
        // Global colliders accumulated automatically
        protected readonly List<AABB> Colliders = new List<AABB>();
        protected readonly List<Bullet> Bullets = new List<Bullet>();

        // Global draw distance
        public static float DrawDistance =150f; // default draw distance
        internal static float DrawDistanceSquared { get { return DrawDistance * DrawDistance; } }
        internal static Vector3 CurrentCameraPos; // updated every frame in Update()

        // Trigger system
        protected class TriggerInfo
        {
            public Action<Player> Action;
            public bool TriggerOnce;
            public bool IsTriggered;
            public bool WasColliding;
        }
        protected Dictionary<object, TriggerInfo> _triggers = new Dictionary<object, TriggerInfo>();

        public void AttachTrigger(ISceneEntity entity, Action<Player> action, bool triggerOnce = true)
        {
            if (entity == null || action == null) return;
            if (!_triggers.ContainsKey(entity))
            {
                _triggers[entity] = new TriggerInfo { Action = action, TriggerOnce = triggerOnce, IsTriggered = false, WasColliding = false };
            }
        }



        public List<ISceneEntity> GetEntities() { return Entities; }

        public void AddEntity(ISceneEntity entity, bool collidable = true)
        {
            if (entity == null) return;
            Entities.Add(entity);

            var item = entity as Item;
            if (item != null)
            {
                AttachTrigger(item, (p) => item.OnPickup(p), triggerOnce: true);
            }

            if (!collidable) return;

            var modelEnt = entity as ModelEntity;
            if (modelEnt != null)
            {
                // Ensure model entities participate in collisions when loaded
                modelEnt.AttachCollidersRef(Colliders);
                return;
            }

            var cube = entity as Cube;
            if (cube != null)
            {
                Vector3 size = cube.GetSize();
                Colliders.Add(new AABB(cube.GetPosition(), size, cube));
                return;
            }

            var plane = entity as Plane;
            if (plane != null)
            {
                var size = plane.GetSize();
                if (size != Vector3.Zero)
                {
                    Vector3 rotDeg = plane.GetRotation();
                    float effectiveHeight = size.Y <= 0f ? 0.05f : size.Y;
                    float hx = size.X * 0.5f;
                    float hy = effectiveHeight * 0.5f;
                    float hz = size.Z * 0.5f;
                    Vector3 colliderSize;
                    if (rotDeg != Vector3.Zero)
                    {
                        float ry = MathHelper.DegreesToRadians(rotDeg.Y);
                        float rx = MathHelper.DegreesToRadians(rotDeg.X);
                        float rz = MathHelper.DegreesToRadians(rotDeg.Z);
                        Matrix4 R = Matrix4.CreateRotationY(ry) * Matrix4.CreateRotationX(rx) * Matrix4.CreateRotationZ(rz);
                        Vector4 r0 = R.Row0; Vector4 r1 = R.Row1; Vector4 r2 = R.Row2;
                        float ex = Math.Abs(r0.X) * hx + Math.Abs(r0.Y) * hy + Math.Abs(r0.Z) * hz;
                        float ey = Math.Abs(r1.X) * hx + Math.Abs(r1.Y) * hy + Math.Abs(r1.Z) * hz;
                        float ez = Math.Abs(r2.X) * hx + Math.Abs(r2.Y) * hy + Math.Abs(r2.Z) * hz;
                        colliderSize = new Vector3(ex * 2f, ey * 2f, ez * 2f);
                    }
                    else
                    {
                        colliderSize = new Vector3(hx * 2f, hy * 2f, hz * 2f);
                    }
                    Colliders.Add(new AABB(plane.GetPosition(), colliderSize, plane));
                }
                return;
            }

            var checkboardTerrain = entity as CheckboardTerrain;
            if (checkboardTerrain != null)
            {
                Vector3 size = checkboardTerrain.GetSize();
                const float groundThickness = 0.2f; // thin collision layer
                Colliders.Add(new AABB(checkboardTerrain.GetPosition(), new Vector3(size.X, groundThickness, size.Z), checkboardTerrain));
                return;
            }

            var sphere = entity as Sphere;
            if (sphere != null) {
                float radius = sphere.GetRadius();
                Colliders.Add(new AABB(sphere.GetPosition(), new Vector3(radius * 2f, radius * 2f, radius * 2f), sphere));
                return;
            }

            var npc = entity as NPC;
            if (npc != null)
            {
                Colliders.Add(npc.GetCollider());
                return;
            }
        }

        public void RemoveEntity(ISceneEntity entity)
        {
            if (entity == null) return;
            Entities.Remove(entity);
            Colliders.Clear();
            var currentEntities = new List<ISceneEntity>(Entities);
            Entities.Clear();
            foreach (var ent in currentEntities)
            {
                AddEntity(ent, true);
            }
        }

        // Per-frame logic hook for scenes
        public virtual void Update(float dt, Player player, Camera camera, KeyboardState currentKeyboard, KeyboardState previousKeyboard, MouseState currentMouse, MouseState previousMouse)
        {
            // Update camera position reference for distance culling
            CurrentCameraPos = camera != null ? camera.position : Vector3.Zero;

            for (int i = 0; i < Entities.Count; i++)
            {
                var me = Entities[i] as ModelEntity;
                if (me != null)
                    me.Evaluate(CurrentCameraPos);

                var item = Entities[i] as Item;
                if (item != null)
                    item.Update(dt);

                var npc = Entities[i] as NPC;
                if (npc != null)
                {
                    npc.Update(dt, player);
                    npc.ResolveCollisions(Colliders, player);
                }
            }

            // Handle shooting only when input is enabled (window focused & console closed)
            if (player != null)
            {
                if (Engine.InputEnabled)
                {
                    Bullet b = player.CheckShoot(currentMouse, previousMouse);
                    if (b != null)
                    {
                        Bullets.Add(b);
                    }
                    // After a focused frame, clear one-shot block if it was set but no click occurred
                    if (Engine.BlockShootOnce && !(currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released))
                    {
                        Engine.BlockShootOnce = false;
                    }
                }
                else
                {
                    // keep previous mouse state sync to avoid stale edge when focus returns
                }
            }

            // Update bullets
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                Bullets[i].Update(dt, Colliders);
                if (Bullets[i].IsDead)
                {
                    Bullets.RemoveAt(i);
                }
            }

            // Check for triggers
            if (player != null)
            {
                AABB playerHitbox = player.Hitbox();
                foreach (var collider in Colliders)
                {
                    if (collider.Owner != null && _triggers.TryGetValue(collider.Owner, out TriggerInfo info))
                    {
                        bool isColliding = playerHitbox.Intersects(collider);

                        if (isColliding && !info.WasColliding)
                        {
                            if (!info.TriggerOnce || !info.IsTriggered)
                            {
                                info.Action(player);
                                info.IsTriggered = true;
                            }
                        }
                        info.WasColliding = isColliding;
                    }
                }
            }
        }

        public virtual void Render()
        {
            for (int i =0; i < Entities.Count; i++)
            {
                Entities[i].Render();
            }

            for (int i = 0; i < Bullets.Count; i++)
            {
                Bullets[i].Render();
            }
        }

        public virtual void Dispose()
        {
            for (int i =0; i < Entities.Count; i++)
            {
                var disp = Entities[i] as IDisposable;
                if (disp != null)
                {
                    try { disp.Dispose(); } catch { }
                }
            }
        }
    }
}
