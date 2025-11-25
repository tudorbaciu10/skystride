using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace skystride.shaders
{
    internal class Lightning
    {
        public bool enabled;
        public Lightning()
        {
            this.enabled = false;
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
            }
        }
    }
}
