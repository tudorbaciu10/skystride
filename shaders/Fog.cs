using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace skystride.shaders
{
    internal class Fog
    {
        private Color color;
        private FogMode mode;
        private float density;
        private float start;
        private float end;

        public Fog() : this(Color.LightBlue, FogMode.Exp2, 0.015f, 50f, 300f){}

        public Fog(Color _color, FogMode _mode, float _density, float _start, float _end)
        {
            this.color = _color;
            this.mode = _mode;
            this.density = _density < 0f ? 0f : _density;
            this.start = _start;
            this.end = _end;
        }

        public void Render()
        {
            GL.Enable(EnableCap.Fog);

            GL.Fog(FogParameter.FogMode, (int)this.mode);

            float[] color = { this.color.R / 255f, this.color.G / 255f, this.color.B / 255f, 1f };
            GL.Fog(FogParameter.FogColor, color);

            GL.Hint(HintTarget.FogHint, HintMode.Nicest);

            GL.Fog(FogParameter.FogDensity, this.density);
            GL.Fog(FogParameter.FogStart, this.start);
            GL.Fog(FogParameter.FogEnd, this.end);
        }
    }
}
