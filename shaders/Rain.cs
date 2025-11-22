using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using skystride.scenes;

namespace skystride.shaders
{
    internal class Rain : ISceneEntity
    {
        private struct Particle
        {
            public Vector3 Position;
            public float Speed; // downward speed
            public float Size; // point size
        }

        private Particle[] _particles;
        private readonly Random _rng = new Random();
        private readonly float _areaSize;
        private readonly float _spawnHeight;
        private readonly float _groundY;
        private readonly int _count;
        private readonly float _minSpeed;
        private readonly float _maxSpeed;
        private readonly float _minSize;
        private readonly float _maxSize;
        private readonly float _sizeScale;

        private float _minSupportedPointSize = 1f;
        private float _maxSupportedPointSize = 64f;

        private readonly Stopwatch _time = new Stopwatch();
        private double _lastTime;

        // wind drift
        private float _windX;
        private float _windZ;
        private float _windChangeTimer;

        private Vector3 _fallDirection = new Vector3(0f, -1f, 0f); // Default: vertical
        private float _obliqueAngle = 0f; // degrees,0 = vertical

        public float ObliqueAngle
        {
            get => _obliqueAngle;
            set
            {
                _obliqueAngle = value;
                float radians = MathHelper.DegreesToRadians(_obliqueAngle);
                _fallDirection = new Vector3((float)Math.Sin(radians), -(float)Math.Cos(radians), 0f);
                if (_obliqueAngle == 0f) _fallDirection = new Vector3(0f, -1f, 0f);
            }
        }

        public Rain(int count = 2000, float areaSize = 100f, float spawnHeight = 40f, float groundY = -10f,
        float minSpeed = 10.0f, float maxSpeed = 22.0f, float minSize = 1.0f, float maxSize = 2.5f, float sizeScale = 1.2f, float obliqueAngle = 15f)
        {
            _count = count < 10 ? 10 : count;
            _areaSize = areaSize;
            _spawnHeight = spawnHeight;
            _groundY = groundY;
            _minSpeed = minSpeed;
            _maxSpeed = maxSpeed;
            _minSize = minSize <= 0f ? 0.5f : minSize;
            _maxSize = maxSize < _minSize ? _minSize + 0.5f : maxSize;
            _sizeScale = sizeScale < 0.1f ? 0.1f : sizeScale;

            try
            {
                float[] range = new float[2];
                GL.GetFloat(GetPName.AliasedPointSizeRange, range);
                if (range != null && range.Length >= 2)
                {
                    _minSupportedPointSize = range[0];
                    _maxSupportedPointSize = range[1];
                }
            }
            catch { }

            _particles = new Particle[_count];
            InitParticles();
            _time.Start();

            ObliqueAngle = obliqueAngle;
        }

        private void InitParticles()
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                _particles[i].Position = new Vector3(
                RandRange(-_areaSize, _areaSize),
                RandRange(_groundY, _spawnHeight),
                RandRange(-_areaSize, _areaSize));
                _particles[i].Speed = RandRange(_minSpeed, _maxSpeed);
                float t = (_particles[i].Speed - _minSpeed) / ((_maxSpeed - _minSpeed) + 0.0001f);
                _particles[i].Size = RandRange(_minSize, _maxSize) * (0.8f + 0.4f * t) * _sizeScale;
            }
        }

        private float RandRange(float a, float b)
        {
            return (float)(_rng.NextDouble() * (b - a) + a);
        }

        private void Respawn(int i)
        {
            _particles[i].Position = new Vector3(
            RandRange(-_areaSize, _areaSize),
            _spawnHeight + RandRange(0f, 10f),
            RandRange(-_areaSize, _areaSize));
            _particles[i].Speed = RandRange(_minSpeed, _maxSpeed);
            float t = (_particles[i].Speed - _minSpeed) / ((_maxSpeed - _minSpeed) + 0.0001f);
            _particles[i].Size = RandRange(_minSize, _maxSize) * (0.8f + 0.4f * t) * _sizeScale;
        }

        public void Render()
        {
            double now = _time.Elapsed.TotalSeconds;
            float dt = (float)(now - _lastTime);
            _lastTime = now;
            if (dt <= 0f) dt = 1f / 60f;

            UpdateWind(dt);
            UpdateParticles(dt);
            DrawParticles();
        }

        private void UpdateWind(float dt)
        {
            _windChangeTimer -= dt;
            if (_windChangeTimer <= 0f)
            {
                _windX = RandRange(-1.2f, 1.2f);
                _windZ = RandRange(-1.2f, 1.2f);
                _windChangeTimer = RandRange(2f, 5f);
            }
        }

        private void UpdateParticles(float dt)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                var p = _particles[i];
                // Apply oblique direction
                p.Position += _fallDirection * p.Speed * dt;
                float driftScale = 0.6f + (p.Speed - _minSpeed) / (_maxSpeed - _minSpeed + 0.0001f) * 0.8f;
                p.Position.X += _windX * driftScale * dt;
                p.Position.Z += _windZ * driftScale * dt;

                if (p.Position.X < -_areaSize) p.Position.X += _areaSize * 2f;
                else if (p.Position.X > _areaSize) p.Position.X -= _areaSize * 2f;
                if (p.Position.Z < -_areaSize) p.Position.Z += _areaSize * 2f;
                else if (p.Position.Z > _areaSize) p.Position.Z -= _areaSize * 2f;

                if (p.Position.Y <= _groundY)
                {
                    Respawn(i);
                    continue;
                }
                _particles[i] = p;
            }
        }

        private float ClampSize(float s)
        {
            if (s < _minSupportedPointSize) s = _minSupportedPointSize;
            if (s > _maxSupportedPointSize) s = _maxSupportedPointSize;
            return s;
        }

        private void DrawParticles()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            GL.DepthMask(false);

            float sSmall = ClampSize(_minSize * _sizeScale);
            float sMed = ClampSize(((_minSize + _maxSize) * 0.5f) * _sizeScale);
            float sLarge = ClampSize(_maxSize * _sizeScale);
            float t1 = (sSmall + sMed) * 0.5f;
            float t2 = (sMed + sLarge) * 0.5f;

            // small bucket
            GL.PointSize(sSmall);
            GL.Begin(PrimitiveType.Points);
            for (int i = 0; i < _particles.Length; i++)
            {
                var p = _particles[i];
                if (p.Size < t1)
                {
                    GL.Color4(0.5f, 0.7f, 1f, 0.7f);
                    GL.Vertex3(p.Position.X, p.Position.Y, p.Position.Z);
                }
            }
            GL.End();

            // medium bucket
            GL.PointSize(sMed);
            GL.Begin(PrimitiveType.Points);
            for (int i = 0; i < _particles.Length; i++)
            {
                var p = _particles[i];
                if (p.Size >= t1 && p.Size < t2)
                {
                    GL.Color4(0.5f, 0.8f, 1f, 0.8f);
                    GL.Vertex3(p.Position.X, p.Position.Y, p.Position.Z);
                }
            }
            GL.End();

            // large bucket
            GL.PointSize(sLarge);
            GL.Begin(PrimitiveType.Points);
            for (int i = 0; i < _particles.Length; i++)
            {
                var p = _particles[i];
                if (p.Size >= t2)
                {
                    GL.Color4(0.6f, 0.9f, 1f, 0.9f);
                    GL.Vertex3(p.Position.X, p.Position.Y, p.Position.Z);
                }
            }
            GL.End();

            // restore depth writes
            GL.DepthMask(true);
        }
    }
}
