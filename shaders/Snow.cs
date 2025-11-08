using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using skystride.scenes; // for ISceneEntity

namespace skystride.shaders
{
    internal class Snow : ISceneEntity
    {
        private struct Particle
        {
            public Vector3 Position;
            public float Speed; // downward speed (positive value)
            public float Size; // point size (desired)
        }

        private Particle[] _particles;
        private readonly Random _rng = new Random();
        private readonly float _areaSize; // horizontal spawn square edge length /2
        private readonly float _spawnHeight; // top spawn Y
        private readonly float _groundY; // ground Y for reset
        private readonly int _count; // particle count
        private readonly float _minSpeed;
        private readonly float _maxSpeed;
        private readonly float _minSize;
        private readonly float _maxSize;
        private readonly float _sizeScale; // global visibility scale

        private float _minSupportedPointSize = 1f;
        private float _maxSupportedPointSize = 64f;

        private readonly Stopwatch _time = new Stopwatch();
        private double _lastTime;

        // wind drift
        private float _windX;
        private float _windZ;
        private float _windChangeTimer;

        // point sprite texture (circular alpha mask)
        private int _spriteTex;
        private bool _spriteReady;

        public Snow(int count = 1500, float areaSize = 80f, float spawnHeight = 35f, float groundY = 0f,
 float minSpeed = 2.0f, float maxSpeed = 6.0f, float minSize = 2.5f, float maxSize = 5.0f, float sizeScale = 2.0f)
        {
            _count = count < 10 ? 10 : count;
            _areaSize = areaSize;
            _spawnHeight = spawnHeight;
            _groundY = groundY;
            _minSpeed = minSpeed;
            _maxSpeed = maxSpeed;
            _minSize = minSize <= 0f ? 0.5f : minSize;
            _maxSize = maxSize < _minSize ? _minSize + 0.5f : maxSize;
            _sizeScale = sizeScale < 0.1f ? 0.1f : sizeScale; // prevent zero

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
            CreateSpriteTexture();
            _time.Start();
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

        private void CreateSpriteTexture()
        {
            try
            {
                const int W = 64;
                const int H = 64;
                byte[] data = new byte[W * H * 4];
                float cx = (W - 1) * 0.5f;
                float cy = (H - 1) * 0.5f;
                float r = Math.Min(cx, cy) - 1f;
                float r2 = r * r;
                float feather = r * 0.2f; // soft edge
                float inner = r - feather;
                float inner2 = inner * inner;

                int idx = 0;
                for (int y = 0; y < H; y++)
                {
                    for (int x = 0; x < W; x++)
                    {
                        float dx = x - cx;
                        float dy = y - cy;
                        float d2 = dx * dx + dy * dy;
                        byte a;
                        if (d2 <= inner2)
                        {
                            a = 255; // fully opaque
                        }
                        else if (d2 >= r2)
                        {
                            a = 0; // outside circle
                        }
                        else
                        {
                            // smooth falloff between inner and outer radius
                            float t = (float)((Math.Sqrt(d2) - inner) / (r - inner));
                            t = t < 0f ? 0f : (t > 1f ? 1f : t);
                            a = (byte)(255 * (1f - t));
                        }
                        data[idx++] = 255; // R
                        data[idx++] = 255; // G
                        data[idx++] = 255; // B
                        data[idx++] = a; // A
                    }
                }

                int tex;
                GL.GenTextures(1, out tex);
                GL.BindTexture(TextureTarget.Texture2D, tex);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, W, H, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                _spriteTex = tex;
                _spriteReady = true;
            }
            catch
            {
                _spriteReady = false;
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
                _windX = RandRange(-0.8f, 0.8f);
                _windZ = RandRange(-0.8f, 0.8f);
                _windChangeTimer = RandRange(3f, 7f);
            }
        }

        private void UpdateParticles(float dt)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                var p = _particles[i];
                p.Position.Y -= p.Speed * dt;
                float driftScale = 0.4f + (p.Speed - _minSpeed) / (_maxSpeed - _minSpeed + 0.0001f) * 0.6f;
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

            if (_spriteReady)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.PointSprite);
                GL.BindTexture(TextureTarget.Texture2D, _spriteTex);
                GL.TexEnv(TextureEnvTarget.PointSprite, TextureEnvParameter.CoordReplace, 1);
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
            }

            // prepare size buckets to avoid calling PointSize inside Begin/End
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
                    GL.Color4(1f, 1f, 1f, 0.95f);
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
                    GL.Color4(1f, 1f, 1f, 0.98f);
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
                    GL.Color4(1f, 1f, 1f, 1.0f);
                    GL.Vertex3(p.Position.X, p.Position.Y, p.Position.Z);
                }
            }
            GL.End();

            if (_spriteReady)
            {
                GL.TexEnv(TextureEnvTarget.PointSprite, TextureEnvParameter.CoordReplace, 0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.PointSprite);
                GL.Disable(EnableCap.Texture2D);
            }

            // restore depth writes
            GL.DepthMask(true);
        }
    }
}
