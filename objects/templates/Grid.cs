using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skystride.scenes;

namespace skystride.objects.templates
{
    internal class Grid : ISceneEntity
    {
        private readonly Color color;
        private const int STEP = 20;
        private const int UNITS = 50;
        private const int POINT_OFFSET = STEP * UNITS;
        private const int MICRO_OFFSET = 1;

        public Grid()
        {
            this.color = Color.WhiteSmoke;
        }

        public Vector3 GetPosition() { return Vector3.Zero; }
        public void SetPosition(Vector3 pos) { }
        public Vector3 GetSize() { return Vector3.Zero; }
        public void SetSize(Vector3 size) { }

        public void Render()
        {
            GL.Color3(this.color);
            GL.Begin(PrimitiveType.Lines);

            for (int i = -1 * STEP * UNITS; i <= STEP * UNITS; i += STEP)
            {
                // XZ parrallel with OZ axis
                GL.Vertex3(i + MICRO_OFFSET, 0, POINT_OFFSET);
                GL.Vertex3(i + MICRO_OFFSET, 0, -1 * POINT_OFFSET);

                // XZ parrallel with OX axis
                GL.Vertex3(POINT_OFFSET, 0, i + MICRO_OFFSET);
                GL.Vertex3(-1 * POINT_OFFSET, 0, i + MICRO_OFFSET);
            }

            GL.End();
        }
    }
}
