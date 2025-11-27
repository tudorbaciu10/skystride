using OpenTK;
using skystride.scenes;
using skystride.vendor;
using System;

namespace skystride.objects
{
    internal abstract class Item : ModelEntity
    {
        public bool IsActive { get; private set; } = true;

        private float _initialY;
        private float _time;
        private float _floatSpeed = 2.0f;
        private float _floatAmplitude = 0.25f;
        private float _rotationSpeed = 45.0f; // degrees per second

        public Item(string objectPath, string texturePath, Vector3 position, float scale) 
            : base(objectPath, texturePath, position, scale, 0, 0, 0)
        {
            _initialY = position.Y;
        }

        public virtual void Update(float dt)
        {
            if (!IsActive) return;

            _time += dt;

            // Floating logic
            Vector3 pos = GetPosition();
            pos.Y = _initialY + (float)Math.Sin(_time * _floatSpeed) * _floatAmplitude;
            SetPosition(pos);

            // Rotating logic
            Vector3 currentRot = GetRotation();
            float newRotY = currentRot.Y + _rotationSpeed * dt;
            SetRotation(currentRot.X, newRotY, currentRot.Z);
        }

        public override void Render()
        {
            if (!IsActive) return;
            base.Render();
        }

        public abstract void OnPickup(Player player);

        public void SetActive(bool active)
        {
            IsActive = active;
        }
    }
}
