using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using skystride.scenes;
using skystride.vendor.collision;

namespace skystride.objects.templates
{
    internal class Cube : ISceneEntity
    {
        private Vector3 position;
        private float size;
        private Color color;

        public Vector3 GetPosition() { return this.position; }
        public float GetSize() { return this.size; }
        public Color GetColor() { return this.color; }

        public Cube() : this(new Vector3(0f, 0.5f, 0f), 1f, Color.BlueViolet) { }

        public Cube(Vector3 position, float size, Color color)
        {
            this.position = position;
            this.size = size <= 0f ? 1f : size;
            this.color = color;
        }

        public void SetPosition(Vector3 pos) { this.position = pos; }
        public void SetSize(float s) { this.size = s <= 0f ? this.size : s; }
        public void SetColor(Color c) { this.color = c; }

        public void Render()
        {
            float h = this.size * 0.5f;
            float px = position.X, py = position.Y, pz = position.Z;

            Vector3
                v000 = new Vector3(px - h, py - h, pz - h),
                v001 = new Vector3(px - h, py - h, pz + h),
                v010 = new Vector3(px - h, py + h, pz - h),
                v011 = new Vector3(px - h, py + h, pz + h),
                v100 = new Vector3(px + h, py - h, pz - h),
                v101 = new Vector3(px + h, py - h, pz + h),
                v110 = new Vector3(px + h, py + h, pz - h),
                v111 = new Vector3(px + h, py + h, pz + h);

            GL.Color3(this.color);
            GL.Begin(PrimitiveType.Quads);

            // Front (+Z)
            GL.Normal3(0f, 0f, 1f);
            GL.Vertex3(v001);
            GL.Vertex3(v101);
            GL.Vertex3(v111);
            GL.Vertex3(v011);

            // Back (-Z)
            GL.Normal3(0f, 0f, -1f);
            GL.Vertex3(v100);
            GL.Vertex3(v000);
            GL.Vertex3(v010);
            GL.Vertex3(v110);

            // Left (-X)
            GL.Normal3(-1f, 0f, 0f);
            GL.Vertex3(v000);
            GL.Vertex3(v001);
            GL.Vertex3(v011);
            GL.Vertex3(v010);

            // Right (+X)
            GL.Normal3(1f, 0f, 0f);
            GL.Vertex3(v101);
            GL.Vertex3(v100);
            GL.Vertex3(v110);
            GL.Vertex3(v111);

            // Top (+Y)
            GL.Normal3(0f, 1f, 0f);
            GL.Vertex3(v011);
            GL.Vertex3(v111);
            GL.Vertex3(v110);
            GL.Vertex3(v010);

            // Bottom (-Y)
            GL.Normal3(0f, -1f, 0f);
            GL.Vertex3(v000);
            GL.Vertex3(v100);
            GL.Vertex3(v101);
            GL.Vertex3(v001);

            GL.End();
        }
    }
}
