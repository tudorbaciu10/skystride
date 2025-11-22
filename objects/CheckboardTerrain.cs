using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using skystride.scenes;

namespace skystride.objects
{
    internal class CheckboardTerrain : ISceneEntity
    {
        private readonly int tiles;
        private readonly float size;
        private readonly float coord_y;

        private readonly Color lightColor;
        private readonly Color darkColor;

        public CheckboardTerrain(int _tiles = 50, float _tiles_size = 2f, float _coord_y = 0f)
        {
            this.tiles = Math.Max(1, _tiles);
            this.size = Math.Max(0.001f, _tiles_size);
            this.coord_y = _coord_y;
            this.lightColor = Color.FromArgb(215, 215, 215);
            this.darkColor = Color.FromArgb(180, 180, 180);
        }

        public float GetSize()
        {
            return this.size * this.tiles;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(0, this.coord_y, 0);
        }

        public void Render()
        {
            GL.Begin(PrimitiveType.Quads);

            for (int x = -this.tiles; x < this.tiles; x++)
            {
                for (int z = -this.tiles; z < this.tiles; z++)
                {
                    bool isLight = ((x + z) & 1) == 0;
                    var c = isLight ? lightColor : darkColor;
                    GL.Color3(c);

                    float x0 = x * this.size;
                    float x1 = (x + 1) * this.size;
                    float z0 = z * this.size;
                    float z1 = (z + 1) * this.size;

                    GL.Vertex3(x0, this.coord_y, z0);
                    GL.Vertex3(x1, this.coord_y, z0);
                    GL.Vertex3(x1, this.coord_y, z1);
                    GL.Vertex3(x0, this.coord_y, z1);
                }
            }

            GL.End();
        }
    }
}
