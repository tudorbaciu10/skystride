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

        private readonly float[] _globalAmbient = new float[] { 0.12f, 0.12f, 0.12f, 1f };
        private readonly float[] _materialSpecular = new float[] { 1f, 1f, 1f, 1f };
        private readonly float _materialShininess = 32f;

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

                GL.Enable(EnableCap.Normalize);
                GL.ShadeModel(ShadingModel.Smooth);

                // Light model configuration
                GL.LightModel(LightModelParameter.LightModelAmbient, _globalAmbient);
                GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);
                GL.LightModel(LightModelParameter.LightModelTwoSide, 1);

                // Light properties
                GL.Light(LightName.Light0, LightParameter.Position, _lightPosition);
                GL.Light(LightName.Light0, LightParameter.Ambient, _ambient);
                GL.Light(LightName.Light0, LightParameter.Diffuse, _diffuse);
                GL.Light(LightName.Light0, LightParameter.Specular, _specular);

                GL.Enable(EnableCap.ColorMaterial);
                GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, _materialSpecular);
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, _materialShininess);
            }
            else
            {
                GL.Disable(EnableCap.ColorMaterial);
                GL.Disable(EnableCap.Normalize);
                GL.Disable(EnableCap.Light0);
                GL.Disable(EnableCap.Lighting);
            }
        }
    }
}
