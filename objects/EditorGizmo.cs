using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace skystride.objects
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }
    }

    public enum GizmoAxis
    {
        None,
        X,
        Y,
        Z
    }

    public class EditorGizmo
    {
        private const float AxisLength = 2.0f;
        private const float AxisThickness = 0.1f;
        private const float ArrowHeadLen = 0.4f;
        private const float ArrowHeadRadius = 0.2f;

        public GizmoAxis SelectedAxis { get; private set; } = GizmoAxis.None;

        public void Render(Vector3 position)
        {
            GL.Disable(EnableCap.DepthTest); // Always draw on top
            GL.Disable(EnableCap.Lighting);
            GL.PushMatrix();
            GL.Translate(position);

            // X Axis (Red)
            GL.Color3(SelectedAxis == GizmoAxis.X ? Color.Yellow : Color.Red);
            RenderArrow(Vector3.UnitX, AxisLength);

            // Y Axis (Green)
            GL.Color3(SelectedAxis == GizmoAxis.Y ? Color.Yellow : Color.Lime);
            RenderArrow(Vector3.UnitY, AxisLength);

            // Z Axis (Blue)
            GL.Color3(SelectedAxis == GizmoAxis.Z ? Color.Yellow : Color.Blue);
            RenderArrow(Vector3.UnitZ, AxisLength);

            GL.PopMatrix();
            GL.Enable(EnableCap.DepthTest);
        }

        private void RenderArrow(Vector3 dir, float length)
        {
            Vector3 end = dir * length;
            
            // Line
            GL.LineWidth(3f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Vector3.Zero);
            GL.Vertex3(end);
            GL.End();
            GL.LineWidth(1f);

            // Arrowhead (Cone approximation)
            // We need to rotate the cone to match direction
            GL.PushMatrix();
            
            // Calculate rotation to align +Y (default cone up) to 'dir'
            Vector3 up = Vector3.UnitY;
            if (dir != up)
            {
                Vector3 axis = Vector3.Cross(up, dir);
                float angle = (float)MathHelper.RadiansToDegrees(Math.Acos(Vector3.Dot(up, dir)));
                if (axis.LengthSquared < 0.001f) // Parallel but opposite
                {
                    if (Vector3.Dot(up, dir) < 0) axis = Vector3.UnitX;
                }
                GL.Rotate(angle, axis);
            }
            
            GL.Translate(0, length, 0);
            
            // Draw Cone
            int segments = 16;
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Vertex3(0, ArrowHeadLen, 0); // Tip
            for (int i = 0; i <= segments; i++)
            {
                float theta = (float)(i * 2 * Math.PI / segments);
                float x = ArrowHeadRadius * (float)Math.Cos(theta);
                float z = ArrowHeadRadius * (float)Math.Sin(theta);
                GL.Vertex3(x, 0, z);
            }
            GL.End();

            // Cap
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Vertex3(0, 0, 0);
            for (int i = 0; i <= segments; i++)
            {
                float theta = (float)(i * 2 * Math.PI / segments);
                float x = ArrowHeadRadius * (float)Math.Cos(theta);
                float z = ArrowHeadRadius * (float)Math.Sin(theta);
                GL.Vertex3(x, 0, z);
            }
            GL.End();

            GL.PopMatrix();
        }

        public GizmoAxis CheckIntersection(Ray ray, Vector3 position)
        {
            // Transform ray to local space of the gizmo
            Vector3 localOrigin = ray.Origin - position;
            
            // Check intersection with each axis box
            // X Axis Box: (0, -w, -w) to (L, w, w)
            float w = AxisThickness * 2f; // Make it easier to click
            float L = AxisLength + ArrowHeadLen;

            float tX = IntersectBox(localOrigin, ray.Direction, new Vector3(0, -w, -w), new Vector3(L, w, w));
            float tY = IntersectBox(localOrigin, ray.Direction, new Vector3(-w, 0, -w), new Vector3(w, L, w));
            float tZ = IntersectBox(localOrigin, ray.Direction, new Vector3(-w, -w, 0), new Vector3(w, w, L));

            float minT = float.MaxValue;
            GizmoAxis hit = GizmoAxis.None;

            if (tX > 0 && tX < minT) { minT = tX; hit = GizmoAxis.X; }
            if (tY > 0 && tY < minT) { minT = tY; hit = GizmoAxis.Y; }
            if (tZ > 0 && tZ < minT) { minT = tZ; hit = GizmoAxis.Z; }

            return hit;
        }

        public void SetSelectedAxis(GizmoAxis axis)
        {
            SelectedAxis = axis;
        }

        private float IntersectBox(Vector3 origin, Vector3 dir, Vector3 min, Vector3 max)
        {
            Vector3 dirInv = new Vector3(1.0f / dir.X, 1.0f / dir.Y, 1.0f / dir.Z);

            float t1 = (min.X - origin.X) * dirInv.X;
            float t2 = (max.X - origin.X) * dirInv.X;
            float t3 = (min.Y - origin.Y) * dirInv.Y;
            float t4 = (max.Y - origin.Y) * dirInv.Y;
            float t5 = (min.Z - origin.Z) * dirInv.Z;
            float t6 = (max.Z - origin.Z) * dirInv.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            if (tmax < 0) return -1;
            if (tmin > tmax) return -1;

            return tmin;
        }
    }
}
