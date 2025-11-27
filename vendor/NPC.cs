using OpenTK;
using skystride.objects.templates;
using skystride.scenes;
using skystride.vendor.collision;
using System;
using System.Drawing;

namespace skystride.vendor
{
    /// <summary>
    /// Non-Player Character with basic AI intelligence
    /// </summary>
    internal class NPC : ISceneEntity
    {
        /// <summary>
        /// Defines the behavior type of an NPC
        /// </summary>
        public enum NPCType
        {
            /// <summary>
            /// NPC will not attack, only wander
            /// </summary>
            Passive,

            /// <summary>
            /// NPC will attack when provoked or damaged
            /// </summary>
            Defensive,

            /// <summary>
            /// NPC will attack players on sight
            /// </summary>
            Aggressive
        }
        // Visual representation
        private Sphere visualSphere;
        
        // Transform
        private Vector3 position;
        private Vector3 velocity;
        private Vector3 direction;
        
        // NPC properties
        private NPCType npcType;
        private float radius;
        private float moveSpeed;
        private float health;
        
        // AI state
        private float wanderTimer;
        private float wanderInterval;
        private Random random;
        
        // Damage system
        private float damageTimer;
        private float damageCooldown = 1.0f; // 1 second between damage ticks
        private int damagePerHit = 10; // damage dealt per hit
        
        // Physics
        private float gravity = -18.0f;
        private float groundY = -10.0f;
        private float eyeHeight = 0.5f;
        private bool isGrounded = false;

        public NPCType Type { get { return npcType; } }
        public float Health { get { return health; } }

        /// <summary>
        /// Create a new NPC
        /// </summary>
        /// <param name="position">Starting position</param>
        /// <param name="type">Behavior type</param>
        /// <param name="radius">Visual radius (default 0.5f)</param>
        /// <param name="damage">Damage per hit (default 10)</param>
        public NPC(Vector3 position, NPCType type = NPCType.Passive, float radius = 0.5f, int damage = 10)
        {
            this.position = position;
            this.npcType = type;
            this.radius = radius;
            this.moveSpeed = 2.0f; // slower than player
            this.health = 100f;
            this.damagePerHit = damage;
            
            // Initialize damage timer
            this.damageTimer = 0f;
            
            // Create white sphere for visual representation
            this.visualSphere = new Sphere(position, radius, Color.White, 16, 12);
            
            // Initialize AI
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

        /// <summary>
        /// Set the damage per hit for this NPC
        /// </summary>
        public void SetDamage(int damage)
        {
            if (damage >= 0)
            {
                this.damagePerHit = damage;
            }
        }

        /// <summary>
        /// Get the collision box for this NPC
        /// </summary>
        public AABB GetCollider()
        {
            return new AABB(position, new Vector3(radius * 2f, radius * 2f, radius * 2f), this);
        }

        /// <summary>
        /// Update NPC AI and physics
        /// </summary>
        public void Update(float dt, Player player = null)
        {
            if (dt <= 0f) return;

            // Aggressive NPCs chase the player
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
                // Passive and Defensive NPCs wander randomly
                wanderTimer += dt;
                if (wanderTimer >= wanderInterval)
                {
                    // Change direction
                    direction = GetRandomDirection();
                    wanderTimer = 0f;
                    wanderInterval = 2.0f + (float)random.NextDouble() * 2.0f;
                }
            }

            // Apply movement in current direction
            Vector3 wishDir = direction;
            wishDir.Y = 0f; // keep movement horizontal
            if (wishDir.LengthSquared > 0f)
            {
                wishDir.NormalizeFast();
            }

            // Simple velocity update (no advanced physics like player)
            if (isGrounded)
            {
                velocity.X = wishDir.X * moveSpeed;
                velocity.Z = wishDir.Z * moveSpeed;
            }

            // Apply gravity
            velocity.Y += gravity * dt;

            // Update position
            position += velocity * dt;

            // Ground collision (simple plane check)
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

            // Check for player collision and deal damage
            if (player != null && npcType == NPCType.Aggressive)
            {
                AABB playerHitbox = player.Hitbox();
                AABB npcHitbox = GetCollider();
                
                if (playerHitbox.Intersects(npcHitbox))
                {
                    // Deal damage immediately on first contact (timer == 0) or after cooldown
                    if (damageTimer == 0f || damageTimer >= damageCooldown)
                    {
                        player.TakeDamage(damagePerHit);
                        damageTimer = 0.001f; // Set to small value to prevent immediate re-trigger
                    }
                    
                    // Update damage timer
                    damageTimer += dt;
                }
                else
                {
                    // Reset timer when not in contact
                    damageTimer = 0f;
                }
            }

            // Update visual sphere position
            if (visualSphere != null)
            {
                visualSphere.SetPosition(position);
            }
        }

        /// <summary>
        /// Resolve collisions with environment and player
        /// </summary>
        public void ResolveCollisions(System.Collections.Generic.IEnumerable<AABB> colliders, Player player = null)
        {
            if (colliders == null) return;

            float halfSize = radius;
            Vector3 p = position;
            bool groundedThisFrame = false;

            // First, check collision with player
            if (player != null)
            {
                AABB playerHitbox = player.Hitbox();
                Vector3 npcMin = new Vector3(p.X - halfSize, p.Y - halfSize, p.Z - halfSize);
                Vector3 npcMax = new Vector3(p.X + halfSize, p.Y + halfSize, p.Z + halfSize);
                Vector3 colMin = playerHitbox.Min;
                Vector3 colMax = playerHitbox.Max;

                // Check if colliding with player
                if (!(npcMax.X <= colMin.X || npcMin.X >= colMax.X ||
                      npcMax.Y <= colMin.Y || npcMin.Y >= colMax.Y ||
                      npcMax.Z <= colMin.Z || npcMin.Z >= colMax.Z))
                {
                    // Colliding with player - push NPC away horizontally
                    float penX = Math.Min(npcMax.X - colMin.X, colMax.X - npcMin.X);
                    float penZ = Math.Min(npcMax.Z - colMin.Z, colMax.Z - npcMin.Z);

                    if (penX < penZ)
                    {
                        // Resolve along X
                        Vector3 playerCenter = (colMin + colMax) * 0.5f;
                        if (position.X < playerCenter.X)
                            p.X = colMin.X - halfSize;
                        else
                            p.X = colMax.X + halfSize;
                    }
                    else
                    {
                        // Resolve along Z
                        Vector3 playerCenter = (colMin + colMax) * 0.5f;
                        if (position.Z < playerCenter.Z)
                            p.Z = colMin.Z - halfSize;
                        else
                            p.Z = colMax.Z + halfSize;
                    }
                }
            }

            // Then check environment collisions

            foreach (var c in colliders)
            {
                if (c == null) continue;
                if (c.Owner == this) continue; // Don't collide with self

                // Compute NPC box and collider box
                Vector3 npcMin = new Vector3(p.X - halfSize, p.Y - halfSize, p.Z - halfSize);
                Vector3 npcMax = new Vector3(p.X + halfSize, p.Y + halfSize, p.Z + halfSize);
                Vector3 colMin = c.Min;
                Vector3 colMax = c.Max;

                // Quick reject
                if (npcMax.X <= colMin.X || npcMin.X >= colMax.X ||
                    npcMax.Y <= colMin.Y || npcMin.Y >= colMax.Y ||
                    npcMax.Z <= colMin.Z || npcMin.Z >= colMax.Z)
                {
                    continue;
                }

                // Horizontal overlap
                bool overlapX = npcMax.X > colMin.X && npcMin.X < colMax.X;
                bool overlapZ = npcMax.Z > colMin.Z && npcMin.Z < colMax.Z;

                // Vertical resolution (standing on top)
                if (overlapX && overlapZ)
                {
                    // Landing on top
                    if (npcMin.Y < colMax.Y && npcMax.Y > colMax.Y)
                    {
                        p.Y = colMax.Y + halfSize;
                        if (velocity.Y < 0f) velocity.Y = 0f;
                        groundedThisFrame = true;
                        npcMin.Y = p.Y - halfSize;
                        npcMax.Y = p.Y + halfSize;
                        continue;
                    }

                    // Hitting head
                    if (npcMax.Y > colMin.Y && npcMin.Y < colMin.Y)
                    {
                        p.Y = colMin.Y - halfSize;
                        if (velocity.Y > 0f) velocity.Y = 0f;
                        npcMin.Y = p.Y - halfSize;
                        npcMax.Y = p.Y + halfSize;
                        continue;
                    }
                }

                // Horizontal resolution
                float penX = Math.Min(npcMax.X - colMin.X, colMax.X - npcMin.X);
                float penZ = Math.Min(npcMax.Z - colMin.Z, colMax.Z - npcMin.Z);

                if (penX < penZ)
                {
                    // Resolve along X
                    Vector3 colCenter = (colMin + colMax) * 0.5f;
                    if (position.X < colCenter.X)
                        p.X = colMin.X - halfSize;
                    else
                        p.X = colMax.X + halfSize;
                    
                    // Bounce off wall - change direction
                    direction.X = -direction.X;
                }
                else
                {
                    // Resolve along Z
                    Vector3 colCenter = (colMin + colMax) * 0.5f;
                    if (position.Z < colCenter.Z)
                        p.Z = colMin.Z - halfSize;
                    else
                        p.Z = colMax.Z + halfSize;
                    
                    // Bounce off wall - change direction
                    direction.Z = -direction.Z;
                }
            }

            if (groundedThisFrame)
            {
                isGrounded = true;
            }

            position = p;
            if (visualSphere != null)
            {
                visualSphere.SetPosition(position);
            }
        }

        /// <summary>
        /// Take damage
        /// </summary>
        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health < 0f) health = 0f;

            // TODO: Implement defensive/aggressive response
        }

        /// <summary>
        /// Check if NPC is dead
        /// </summary>
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
        }

        /// <summary>
        /// Generate a random horizontal direction
        /// </summary>
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
