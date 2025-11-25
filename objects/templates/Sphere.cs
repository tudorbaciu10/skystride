using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using skystride.scenes;

namespace skystride.objects.templates
{
    internal class Sphere : ISceneEntity
    {
        private Vector3 position;
        private float radius;
        private Color color;
        private int slices; // longitudinal divisions
        private int stacks; // latitudinal divisions

        public Vector3 GetPosition() { return this.position; }
        public float GetRadius() { return this.radius; }
        public Color GetColor() { return this.color; }

        public Sphere() : this(new Vector3(0f, 0.5f, 0f), 0.5f, Color.OrangeRed) { }

        public Sphere(Vector3 position, float radius, Color color, int slices = 24, int stacks = 16)
        {
            this.position = position;
            this.radius = radius <= 0f ? 0.5f : radius;
            this.color = color;
            this.slices = Math.Max(3, slices);
            this.stacks = Math.Max(2, stacks);
        }

        public void SetPosition(Vector3 pos) { this.position = pos; }
        public void SetRadius(float r) { if (r > 0f) this.radius = r; }
        public void SetColor(Color c) { this.color = c; }
        public void SetTessellation(int slices, int stacks)
        {
            if (slices >= 3) this.slices = slices;
            if (stacks >= 2) this.stacks = stacks;
        }

        public void Render()
        {
            GL.Color3(this.color);

            float dTheta = (float)(2.0 * Math.PI / this.slices);
            float dPhi = (float)(Math.PI / this.stacks); // from -PI/2 to +PI/2

            // phi from -PI/2 (south pole) to +PI/2 (north pole)
            float phiStart = (float)(-0.5 * Math.PI);

            for (int i = 0; i < this.stacks; i++)
            {
                float phi0 = phiStart + i * dPhi;
                float phi1 = phi0 + dPhi;

                GL.Begin(PrimitiveType.QuadStrip);
                for (int j = 0; j <= this.slices; j++)
                {
                    float theta = j * dTheta;

                    // First vertex on the current latitude (phi0)
                    float c0 = (float)Math.Cos(phi0);
                    float s0 = (float)Math.Sin(phi0);
                    float x0 = c0 * (float)Math.Cos(theta);
                    float y0 = s0;
                    float z0 = c0 * (float)Math.Sin(theta);
                    Vector3 n0 = new Vector3(x0, y0, z0);
                    Vector3 v0 = this.position + n0 * this.radius;
                    GL.Normal3(n0);
                    GL.Vertex3(v0);

                    // Second vertex on the next latitude (phi1)
                    float c1 = (float)Math.Cos(phi1);
                    float s1 = (float)Math.Sin(phi1);
                    float x1 = c1 * (float)Math.Cos(theta);
                    float y1 = s1;
                    float z1 = c1 * (float)Math.Sin(theta);
                    Vector3 n1 = new Vector3(x1, y1, z1);
                    Vector3 v1 = this.position + n1 * this.radius;
                    GL.Normal3(n1);
                    GL.Vertex3(v1);
                }
                GL.End();
            }
        }
    }
}
