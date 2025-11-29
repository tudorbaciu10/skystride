using System;
using OpenTK;
using skystride.vendor;

namespace skystride.objects.weapons
{
    internal class Weapon
    {
        protected int ammo, damage;
        protected string name;
        protected Model model;

        public string ModelPath { get; protected set; }
        public string TexturePath { get; protected set; }
        public float Scale { get { return scale; } }

        protected Vector3 viewOffset = new Vector3(0.6f, -0.6f, -1.6f);
        protected Vector3 spawnOffset = new Vector3(0.2f, -0.2f, -0.5f);
        protected Vector3 rotation = Vector3.Zero;
        protected float scale = 0.25f;
        protected float recoilForce = 0f;

        // Animation state
        protected Vector3 recoilOffset = Vector3.Zero;
        protected Vector3 recoilRotation = Vector3.Zero;
        
        // Configuration
        protected float recoilRecoverySpeed = 5.0f;
        protected float recoilKickBack = 0.2f;
        protected float recoilKickUp = 10.0f; // degrees

        public Weapon(string name, int ammo, int damage)
        {
            this.name = name;
            this.ammo = ammo;
            this.damage = damage;
        }

        public int Ammo { get { return ammo; } }
        public string Name { get { return name; } }
        public float RecoilForce { get { return recoilForce; } }

        public void AddAmmo(int amount)
        {
            if (amount > 0)
            {
                ammo += amount;
            }
        }

        public virtual void Update(float dt)
        {
            // Recover from recoil
            recoilOffset = Vector3.Lerp(recoilOffset, Vector3.Zero, dt * recoilRecoverySpeed);
            recoilRotation = Vector3.Lerp(recoilRotation, Vector3.Zero, dt * recoilRecoverySpeed);
        }

        public virtual void Render(Camera _camera)
        {
            if (model == null || _camera == null) return;

            float rotX = rotation.X + recoilRotation.X;
            float rotY = rotation.Y + recoilRotation.Y;
            float rotZ = rotation.Z + recoilRotation.Z;
            
            Vector3 finalPos = viewOffset + recoilOffset;

            model.Render(finalPos, scale, rotX, rotY, rotZ);
        }

        public virtual Bullet Shoot(Vector3 playerPos, Vector3 front, Vector3 up, Vector3 right)
        {
            if (ammo <= 0) return null;
            ammo--;

            // Trigger recoil
            recoilOffset.Z += recoilKickBack; // Move back
            recoilRotation.X += recoilKickUp; // Rotate up (inverted from -= to make it rise)

            // viewOffset is in camera space: X=Right, Y=Up, Z=Back (so -Z is Front)
            // Use spawnOffset for bullet origin to prevent clipping
            Vector3 muzzlePos = playerPos
                              + right * spawnOffset.X
                              + up * spawnOffset.Y
                              - front * spawnOffset.Z; 

            float speed = 100.0f;
            float lifetime = 3.0f;
            return new Bullet(muzzlePos, front, speed, lifetime);
        }

        public virtual void OnRightClick(Player player) { }

        public virtual void RenderUI(int width, int height) { }

        public Vector3 ItemRotation { get; protected set; } = Vector3.Zero;
        public Vector3 ItemRotationSpeed { get; protected set; } = new Vector3(0, 45.0f, 0);

        public virtual float GetDesiredFov() { return 60.0f; }
    }
}