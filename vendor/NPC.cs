using OpenTK;
using skystride.objects.templates;
using skystride.scenes;
using skystride.vendor.collision;
using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace skystride.vendor
{

    internal class NPC : ISceneEntity
    {
        public enum NPCType
        {
            Passive,
            Defensive,
            Aggressive
        }

        private Sphere visualSphere;

        private Vector3 position;
        private Vector3 velocity;
        private Vector3 direction;

        private NPCType npcType;
        private string name;
        private float radius;
        private float moveSpeed;
        private float health;

        private AABB collider;

        private float wanderTimer;
        private float wanderInterval;
        private Random random;

        private float damageTimer;
        private float damageCooldown = 1.0f; // 1 second between damage ticks
        private int damagePerHit = 10; // damage dealt per hit

        private float gravity = -18.0f;
        private float groundY = -10.0f;
        private float eyeHeight = 0.5f;
        private bool isGrounded = false;

        public NPCType Type { get { return npcType; } }
        public float Health { get { return health; } }

        public Action<Vector3> OnDeath { get; set; }

        public NPC(Vector3 position, string name = "NPC", float health = 100f, float size = 1.0f, NPCType type = NPCType.Passive, int damage = 10)
        {
            this.position = position;
            this.npcType = type;
            this.name = name;
            this.radius = size * 0.5f;
            this.moveSpeed = 2.0f; // slower than player
            this.health = health;
            this.damagePerHit = damage;

            this.damageTimer = 0f;

            this.collider = new AABB(position, new Vector3(radius * 2f), this);

            this.visualSphere = new Sphere(position, radius, Color.White, 16, 12);

            this.random = new Random(Guid.NewGuid().GetHashCode());
            this.wanderInterval = 2.0f + (float)random.NextDouble() * 2.0f; // 2-4 seconds
            this.wanderTimer = 0f;
            this.direction = GetRandomDirection();
            this.velocity = Vector3.Zero;
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public void SetPosition(Vector3 pos)
        {
            this.position = pos;
            if (visualSphere != null)
            {
                visualSphere.SetPosition(pos);
            }
        }

        public Vector3 GetSize()
        {
            return new Vector3(radius * 2f, radius * 2f, radius * 2f);
        }

        public void SetSize(Vector3 size)
        {
            if (size.X > 0f)
            {
                this.radius = size.X * 0.5f;
                if (visualSphere != null)
                {
                    visualSphere.SetRadius(this.radius);
                }
            }
        }

        public void SetDamage(int damage)
        {
            if (damage >= 0)
            {
                this.damagePerHit = damage;
            }
        }

        public AABB GetCollider()
        {
            return collider;
        }

        public void Update(float dt, Player player = null)
        {
            if (dt <= 0f) return;

            if (npcType == NPCType.Aggressive && player != null)
            {
                Vector3 playerPos = player.position;
                Vector3 toPlayer = playerPos - position;
                toPlayer.Y = 0f; // keep movement horizontal

                if (toPlayer.LengthSquared > 0f)
                {
                    toPlayer.NormalizeFast();
                    direction = toPlayer;
                }
            }
            else
            {
                wanderTimer += dt;
                if (wanderTimer >= wanderInterval)
                {
                    direction = GetRandomDirection();
                    wanderTimer = 0f;
                    wanderInterval = 2.0f + (float)random.NextDouble() * 2.0f;
                }
            }

            Vector3 wishDir = direction;
            wishDir.Y = 0f; // keep movement horizontal
            if (wishDir.LengthSquared > 0f)
            {
                wishDir.NormalizeFast();
            }

            if (isGrounded)
            {
                velocity.X = wishDir.X * moveSpeed;
                velocity.Z = wishDir.Z * moveSpeed;
            }

            velocity.Y += gravity * dt;

            position += velocity * dt;

            float minY = groundY + eyeHeight;
            if (position.Y <= minY)
            {
                position.Y = minY;
                if (velocity.Y < 0f)
                {
                    velocity.Y = 0f;
                }
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            if (player != null && npcType == NPCType.Aggressive)
            {
                AABB playerHitbox = player.Hitbox();
                AABB npcHitbox = GetCollider();

                if (playerHitbox.Intersects(npcHitbox))
                {
                    if (damageTimer == 0f || damageTimer >= damageCooldown)
                    {
                        player.TakeDamage(damagePerHit);
                        damageTimer = 0.001f; // Set to small value to prevent immediate re-trigger
                    }

                    damageTimer += dt;
                }
                else
                {
                    damageTimer = 0f;
                }
            }

            if (visualSphere != null)
            {
                visualSphere.SetPosition(position);
            }
        }

        public void ResolveCollisions(System.Collections.Generic.IEnumerable<AABB> colliders, Player player = null)
        {
            if (colliders == null) return;

            float halfSize = radius;
            Vector3 p = position;
            bool groundedThisFrame = false;

            if (player != null)
            {
                AABB playerHitbox = player.Hitbox();
                Vector3 npcMin = new Vector3(p.X - halfSize, p.Y - halfSize, p.Z - halfSize);
                Vector3 npcMax = new Vector3(p.X + halfSize, p.Y + halfSize, p.Z + halfSize);
                Vector3 colMin = playerHitbox.Min;
                Vector3 colMax = playerHitbox.Max;

                if (!(npcMax.X <= colMin.X || npcMin.X >= colMax.X ||
                      npcMax.Y <= colMin.Y || npcMin.Y >= colMax.Y ||
                      npcMax.Z <= colMin.Z || npcMin.Z >= colMax.Z))
                {
                    float penX = Math.Min(npcMax.X - colMin.X, colMax.X - npcMin.X);
                    float penZ = Math.Min(npcMax.Z - colMin.Z, colMax.Z - npcMin.Z);

                    if (penX < penZ)
                    {
                        Vector3 playerCenter = (colMin + colMax) * 0.5f;
                        if (position.X < playerCenter.X)
                            p.X = colMin.X - halfSize;
                        else
                            p.X = colMax.X + halfSize;
                    }
                    else
                    {
                        Vector3 playerCenter = (colMin + colMax) * 0.5f;
                        if (position.Z < playerCenter.Z)
                            p.Z = colMin.Z - halfSize;
                        else
                            p.Z = colMax.Z + halfSize;
                    }
                }
            }

            foreach (var c in colliders)
            {
                if (c == null) continue;
                if (c.Owner == this) continue; // Don't collide with self

                Vector3 npcMin = new Vector3(p.X - halfSize, p.Y - halfSize, p.Z - halfSize);
                Vector3 npcMax = new Vector3(p.X + halfSize, p.Y + halfSize, p.Z + halfSize);
                Vector3 colMin = c.Min;
                Vector3 colMax = c.Max;

                if (npcMax.X <= colMin.X || npcMin.X >= colMax.X ||
                    npcMax.Y <= colMin.Y || npcMin.Y >= colMax.Y ||
                    npcMax.Z <= colMin.Z || npcMin.Z >= colMax.Z)
                {
                    continue;
                }

                bool overlapX = npcMax.X > colMin.X && npcMin.X < colMax.X;
                bool overlapZ = npcMax.Z > colMin.Z && npcMin.Z < colMax.Z;

                if (overlapX && overlapZ)
                {
                    if (npcMin.Y < colMax.Y && npcMax.Y > colMax.Y)
                    {
                        p.Y = colMax.Y + halfSize;
                        if (velocity.Y < 0f) velocity.Y = 0f;
                        groundedThisFrame = true;
                        npcMin.Y = p.Y - halfSize;
                        npcMax.Y = p.Y + halfSize;
                        continue;
                    }

                    if (npcMax.Y > colMin.Y && npcMin.Y < colMin.Y)
                    {
                        p.Y = colMin.Y - halfSize;
                        if (velocity.Y > 0f) velocity.Y = 0f;
                        npcMin.Y = p.Y - halfSize;
                        npcMax.Y = p.Y + halfSize;
                        continue;
                    }
                }

                float penX = Math.Min(npcMax.X - colMin.X, colMax.X - npcMin.X);
                float penZ = Math.Min(npcMax.Z - colMin.Z, colMax.Z - npcMin.Z);

                if (penX < penZ)
                {
                    Vector3 colCenter = (colMin + colMax) * 0.5f;
                    if (position.X < colCenter.X)
                        p.X = colMin.X - halfSize;
                    else
                        p.X = colMax.X + halfSize;

                    direction.X = -direction.X;
                }
                else
                {
                    Vector3 colCenter = (colMin + colMax) * 0.5f;
                    if (position.Z < colCenter.Z)
                        p.Z = colMin.Z - halfSize;
                    else
                        p.Z = colMax.Z + halfSize;

                    direction.Z = -direction.Z;
                }
            }

            if (groundedThisFrame)
            {
                isGrounded = true;
            }

            position = p;

            if (collider != null)
            {
                collider.Position = position;
            }

            if (visualSphere != null)
            {
                visualSphere.SetPosition(position);
            }
        }

        public void TakeDamage(float damage)
        {
            bool wasAlive = health > 0f;
            health -= damage;
            if (health < 0f) health = 0f;

            if (wasAlive && health <= 0f)
            {
                OnDeath?.Invoke(position);
            }
        }

        public bool IsDead()
        {
            return health <= 0f;
        }

        public void Render()
        {
            if (visualSphere != null)
            {
                visualSphere.Render();
            }

            RenderNameTag();
        }

        private void RenderNameTag()
        {
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            Matrix4 modelViewMatrix, projectionMatrix;
            GL.GetFloat(GetPName.ModelviewMatrix, out modelViewMatrix);
            GL.GetFloat(GetPName.ProjectionMatrix, out projectionMatrix);

            Vector3 headPos = position + new Vector3(0, radius + 0.5f, 0);

            Vector4 clipPos = Vector4.Transform(new Vector4(headPos, 1.0f), modelViewMatrix * projectionMatrix);

            if (clipPos.W > 0)
            {
                Vector3 ndc = clipPos.Xyz / clipPos.W;
                float screenX = viewport[0] + (ndc.X + 1) * 0.5f * viewport[2];
                float screenY = viewport[1] + (1 - ndc.Y) * 0.5f * viewport[3];

                string text = $"{name} ({health} HP)";

                float textWidth = text.Length * 8f;
                screenX -= textWidth / 2f;
                Color color = npcType == NPCType.Aggressive ? Color.Red : Color.LightGreen;

                TextRenderer.RenderText(text, screenX, screenY, color, viewport[2], viewport[3]);
            }
        }

        private Vector3 GetRandomDirection()
        {
            float angle = (float)(random.NextDouble() * Math.PI * 2.0);
            return new Vector3(
                (float)Math.Cos(angle),
                0f,
                (float)Math.Sin(angle)
            );
        }
    }
}
