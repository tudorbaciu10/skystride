using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace skystride.shaders
{
    internal class Lighting
    {
        public bool enabled;

        // Light 0: Sun (Directional)
        private Vector4 _sunPosition = new Vector4(0.5f, 1.0f, 0.5f, 0f); 
        private float[] _sunAmbient = new float[] { 0.0f, 0.0f, 0.0f, 1f };
        private float[] _sunDiffuse = new float[] { 1.0f, 0.95f, 0.8f, 1f };
        private float[] _sunSpecular = new float[] { 1.0f, 0.95f, 0.8f, 1f };

        // Light 1: Sky (Fill)
        private Vector4 _skyPosition = new Vector4(-0.5f, 1.0f, -0.5f, 0f);
        private float[] _skyAmbient = new float[] { 0.0f, 0.0f, 0.0f, 1f };
        private float[] _skyDiffuse = new float[] { 0.3f, 0.35f, 0.45f, 1f };
        private float[] _skySpecular = new float[] { 0.0f, 0.0f, 0.0f, 1f };

        private readonly float[] _globalAmbient = new float[] { 0.2f, 0.2f, 0.2f, 1f };
        private readonly float[] _materialSpecular = new float[] { 0.5f, 0.5f, 0.5f, 1f };
        private readonly float _materialShininess = 16f;

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
                GL.Enable(EnableCap.Light0); // Sun
                GL.Enable(EnableCap.Light1); // Sky

                GL.Enable(EnableCap.Normalize);
                GL.ShadeModel(ShadingModel.Smooth);

                // Light model configuration
                GL.LightModel(LightModelParameter.LightModelAmbient, _globalAmbient);
                GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);
                GL.LightModel(LightModelParameter.LightModelTwoSide, 0); // Usually 0 for performance unless needed

                // Sun Properties (Light0)
                GL.Light(LightName.Light0, LightParameter.Position, _sunPosition);
                GL.Light(LightName.Light0, LightParameter.Ambient, _sunAmbient);
                GL.Light(LightName.Light0, LightParameter.Diffuse, _sunDiffuse);
                GL.Light(LightName.Light0, LightParameter.Specular, _sunSpecular);

                // Sky Properties (Light1)
                GL.Light(LightName.Light1, LightParameter.Position, _skyPosition);
                GL.Light(LightName.Light1, LightParameter.Ambient, _skyAmbient);
                GL.Light(LightName.Light1, LightParameter.Diffuse, _skyDiffuse);
                GL.Light(LightName.Light1, LightParameter.Specular, _skySpecular);

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
                GL.Disable(EnableCap.Light1);
                GL.Disable(EnableCap.Lighting);
            }
        }
    }
}
