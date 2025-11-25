using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace skystride.shaders
{
    internal class Lighting
    {
        public bool enabled;
        private Vector4 _lightPosition = new Vector4(60f, 120f, 60f, 1f);

        private float[] _ambient = new float[] { 0.35f, 0.28f, 0.15f, 1f };
        private float[] _diffuse = new float[] { 1.25f, 1.10f, 0.65f, 1f };
        private float[] _specular = new float[] { 1.15f, 1.0f, 0.55f, 1f };

        public Lighting()
        {
            this.enabled = true;
        }

        public void Toggle()
        {
            this.enabled = !this.enabled;
        }

        public void Enable()
        {
            this.enabled = true;
        }

        public void Disable()
        {
            this.enabled = false;
        }

        public void Render()
        {
            if (this.enabled)
            {
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);

                GL.Light(LightName.Light0, LightParameter.Position, _lightPosition);
                GL.Light(LightName.Light0, LightParameter.Ambient, _ambient);
                GL.Light(LightName.Light0, LightParameter.Diffuse, _diffuse);
                GL.Light(LightName.Light0, LightParameter.Specular, _specular);
            }
            else
            {
                GL.Disable(EnableCap.Light0);
                GL.Disable(EnableCap.Lighting);
            }
        }
    }
}
