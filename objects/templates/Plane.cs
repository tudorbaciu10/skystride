using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using skystride.scenes;

namespace skystride.objects.templates
{
    internal class Plane : ISceneEntity
    {
        private Vector3 position; // Center position of plane
        private float width; // Size along X axis
        private float depth; // Size along Z axis
        private float height; // Size along Y axis (thickness); if <=0 a single quad is rendered
        private Color color;
        private Vector3 normal; // Surface normal used for lighting (top face normal)

        public Plane() : this(new Vector3(0f,0f,0f),1f,1f,1f, Color.LightGray, new Vector3(0f,1f,0f)) { }

        // Backwards compatible ctor (no height -> flat plane)
        public Plane(Vector3 position, float width, float depth, Color color, Vector3 normal)
         : this(position, width, depth,0f, color, normal) { }

        public Plane(Vector3 position, float width, float depth, float height, Color color, Vector3 normal)
        {
            this.position = position;
            this.width = width <=0f ?1f : width;
            this.depth = depth <=0f ?1f : depth;
            this.height = height <0f ?0f : height; // negative height coerced to0 (flat)
            this.color = color;
            this.normal = normal.LengthSquared >0f ? Vector3.Normalize(normal) : new Vector3(0f,1f,0f);
        }

        public void SetPosition(Vector3 pos) { this.position = pos; }
        public void SetSize(float width, float depth)
        {
            if (width >0f) this.width = width;
            if (depth >0f) this.depth = depth;
        }
        public void SetSize(float width, float depth, float height)
        {
            SetSize(width, depth);
            SetHeight(height);
        }
        public void SetHeight(float h)
        {
            if (h >=0f) this.height = h; // allow0 -> flat
        }
        public void SetColor(Color c) { this.color = c; }
        public void SetNormal(Vector3 n)
        {
            if (n.LengthSquared >0f) this.normal = Vector3.Normalize(n);
        }

        public void Render()
        {
            float hw = this.width *0.5f; // Half width
            float hd = this.depth *0.5f; // Half depth
            float hh = this.height *0.5f; // Half height/thickness
            float px = position.X, py = position.Y, pz = position.Z;

            // Top (if thickness) and/or single quad plane
            Vector3 top00 = new Vector3(px - hw, py + hh, pz - hd);
            Vector3 top01 = new Vector3(px - hw, py + hh, pz + hd);
            Vector3 top10 = new Vector3(px + hw, py + hh, pz - hd);
            Vector3 top11 = new Vector3(px + hw, py + hh, pz + hd);

            GL.Color3(this.color);
            GL.Begin(PrimitiveType.Quads);

            if (this.height <=0f)
            {
                // Flat single quad (legacy behavior)
                GL.Normal3(this.normal);
                GL.Vertex3(top00);
                GL.Vertex3(top10);
                GL.Vertex3(top11);
                GL.Vertex3(top01);
                GL.End();
                return;
            }

            // Compute bottom face vertices
            Vector3 bottom00 = new Vector3(px - hw, py - hh, pz - hd);
            Vector3 bottom01 = new Vector3(px - hw, py - hh, pz + hd);
            Vector3 bottom10 = new Vector3(px + hw, py - hh, pz - hd);
            Vector3 bottom11 = new Vector3(px + hw, py - hh, pz + hd);

            // Top face
            GL.Normal3(this.normal);
            GL.Vertex3(top00);
            GL.Vertex3(top10);
            GL.Vertex3(top11);
            GL.Vertex3(top01);

            // Bottom face (inverse normal)
            GL.Normal3(-this.normal);
            GL.Vertex3(bottom10);
            GL.Vertex3(bottom00);
            GL.Vertex3(bottom01);
            GL.Vertex3(bottom11);

            // Sides (approximate normals using axis-aligned assumptions; for non Y-up normals lighting may be off)
            // +X side
            GL.Normal3(1f,0f,0f);
            GL.Vertex3(top10);
            GL.Vertex3(bottom10);
            GL.Vertex3(bottom11);
            GL.Vertex3(top11);
            // -X side
            GL.Normal3(-1f,0f,0f);
            GL.Vertex3(bottom00);
            GL.Vertex3(top00);
            GL.Vertex3(top01);
            GL.Vertex3(bottom01);
            // +Z side
            GL.Normal3(0f,0f,1f);
            GL.Vertex3(top01);
            GL.Vertex3(top11);
            GL.Vertex3(bottom11);
            GL.Vertex3(bottom01);
            // -Z side
            GL.Normal3(0f,0f, -1f);
            GL.Vertex3(top10);
            GL.Vertex3(top00);
            GL.Vertex3(bottom00);
            GL.Vertex3(bottom10);

            GL.End();
        }
    }
}
