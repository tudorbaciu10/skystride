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

        protected Vector3 viewOffset = new Vector3(0.6f, -0.6f, -1.6f);
        protected Vector3 rotation = Vector3.Zero;
        protected float scale = 0.25f;

        public Weapon(string name, int ammo, int damage)
        {
            this.name = name;
            this.ammo = ammo;
            this.damage = damage;
        }

        public int Ammo { get { return ammo; } }

        public virtual void Render(Camera _camera)
        {
            if (model == null || !model.Loaded || _camera == null) return;

            float rotX = rotation.X;
            float rotY = rotation.Y;
            model.Render(viewOffset, scale, rotX, rotY, rotation.Z);
        }

        public virtual Bullet Shoot(Vector3 playerPos, Vector3 front, Vector3 up, Vector3 right)
        {
            if (ammo <= 0) return null;
            ammo--;

            // viewOffset is in camera space: X=Right, Y=Up, Z=Back (so -Z is Front)
            Vector3 muzzlePos = playerPos
                              + right * viewOffset.X
                              + up * viewOffset.Y
                              - front * viewOffset.Z; // -(-1.6) = +1.6 * front

            float speed = 100.0f;
            float lifetime = 3.0f;
            return new Bullet(muzzlePos, front, speed, lifetime);
        }
    }
}