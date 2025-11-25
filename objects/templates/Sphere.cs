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
        private float height; // vertical radius (Y axis)
        private Color color;
        private int slices; // longitudinal divisions
        private int stacks; // latitudinal divisions

        public Vector3 GetPosition() { return this.position; }
        public float GetRadius() { return this.radius; }
        public float GetHeight() { return this.height; }
        public Color GetColor() { return this.color; }

        public Sphere() : this(new Vector3(0f, 0.5f, 0f), 0.5f, Color.OrangeRed) { }

        // Back-compat: height defaults to radius (perfect sphere)
        public Sphere(Vector3 position, float radius, Color color, int slices = 24, int stacks = 16)
            : this(position, radius, radius, color, slices, stacks)
        {
        }

        // New overload allows specifying vertical radius (height)
        public Sphere(Vector3 position, float radius, float height, Color color, int slices = 24, int stacks = 16)
        {
            this.position = position;
            this.radius = radius <= 0f ? 0.5f : radius;
            this.height = height <= 0f ? this.radius : height;
            this.color = color;
            this.slices = Math.Max(3, slices);
            this.stacks = Math.Max(2, stacks);
        }

        public void SetPosition(Vector3 pos) { this.position = pos; }
        public void SetRadius(float r) { if (r > 0f) this.radius = r; }
        public void SetHeight(float h) { if (h > 0f) this.height = h; }
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

            float rx = this.radius;
            float ry = this.height > 0f ? this.height : this.radius;

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
                    // Vertex scaled as an ellipsoid with radii (rx, ry, rx)
                    Vector3 v0 = new Vector3(
                        this.position.X + x0 * rx,
                        this.position.Y + y0 * ry,
                        this.position.Z + z0 * rx);
                    // Correct normal for an ellipsoid: normalize(x/rx, y/ry, z/rx)
                    Vector3 n0 = new Vector3(
                        rx > 0f ? x0 / rx : x0,
                        ry > 0f ? y0 / ry : y0,
                        rx > 0f ? z0 / rx : z0);
                    n0.Normalize();
                    GL.Normal3(n0);
                    GL.Vertex3(v0);

                    // Second vertex on the next latitude (phi1)
                    float c1 = (float)Math.Cos(phi1);
                    float s1 = (float)Math.Sin(phi1);
                    float x1 = c1 * (float)Math.Cos(theta);
                    float y1 = s1;
                    float z1 = c1 * (float)Math.Sin(theta);
                    Vector3 v1 = new Vector3(
                        this.position.X + x1 * rx,
                        this.position.Y + y1 * ry,
                        this.position.Z + z1 * rx);
                    Vector3 n1 = new Vector3(
                        rx > 0f ? x1 / rx : x1,
                        ry > 0f ? y1 / ry : y1,
                        rx > 0f ? z1 / rx : z1);
                    n1.Normalize();
                    GL.Normal3(n1);
                    GL.Vertex3(v1);
                }
                GL.End();
            }
        }
    }
}
