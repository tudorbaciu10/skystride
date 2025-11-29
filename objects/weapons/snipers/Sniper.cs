using skystride.vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace skystride.objects.weapons.snipers
{
    internal class Sniper : Weapon
    {
        private bool isScoped = false;

        public Sniper() : base("Sniper", 10, 100)
        {
            this.ModelPath = "assets/models/weapons/sniper.obj";
            this.TexturePath = "assets/models/weapons/sniper.jpg";
            this.model = new Model(this.ModelPath, this.TexturePath);
            this.model.SetTextureScale(1f, 1f);

            this.viewOffset = new Vector3(0.9f, -0.7f, -2f);
            this.scale = 0.05f;
            this.rotation = new Vector3(-90f, 0f, 90f);
            this.recoilForce = 10f;
            
            // Item rotation (e.g. lie flat)
            this.ItemRotation = new Vector3(-90f, 0f, 0f);
            this.ItemRotationSpeed = Vector3.Zero; // Disable auto-rotation
        }

        public override void OnRightClick(Player player)
        {
            isScoped = !isScoped;
        }

        public override float GetDesiredFov()
        {
            return isScoped ? 20.0f : 60.0f;
        }

        public override void RenderUI(int width, int height)
        {
            if (!isScoped) return;

            // Draw scope mask (black screen with hole)
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, width, height, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            float centerX = width / 2f;
            float centerY = height / 2f;
            float scopeRadius = height / 2.5f; // Adjust size of the hole
            float outerRadius = width * 1.5f; // Large enough to cover screen corners

            GL.Color4(0f, 0f, 0f, 1f);
            GL.Begin(PrimitiveType.QuadStrip);

            int segments = 64;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * MathHelper.TwoPi;
                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);

                // Inner vertex (on the edge of the hole)
                GL.Vertex2(centerX + cos * scopeRadius, centerY + sin * scopeRadius);
                // Outer vertex (far away)
                GL.Vertex2(centerX + cos * outerRadius, centerY + sin * outerRadius);
            }

            GL.End();

            // Draw Crosshair
            GL.Color4(0f, 0f, 0f, 1f); // Black crosshair
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            
            // Horizontal
            GL.Vertex2(centerX - scopeRadius, centerY);
            GL.Vertex2(centerX + scopeRadius, centerY);

            // Vertical
            GL.Vertex2(centerX, centerY - scopeRadius);
            GL.Vertex2(centerX, centerY + scopeRadius);

            GL.End();

            // Restore state
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }
    }
}
