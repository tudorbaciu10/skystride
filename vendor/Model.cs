using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace skystride.vendor
{
    internal class Model : IDisposable
    {
        private int vbo, nbo, ebo;
        private int indexCount;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<int> indices = new List<int>();

        private Vector3 minBound, maxBound, center, size;
        public bool Loaded { get; private set; }
        public string SourcePath { get; private set; }
        public Vector3 Center { get { return center; } }
        public float BBoxHeight { get { return size.Y; } }

        public Model(string path)
        {
            SourcePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName + path;

            try
            {
                LoadOBJ(SourcePath);
                if (normals.Count == 0)
                    ComputeNormals();
                ComputeBounds();
                UploadToGPU();
                Loaded = true;

                Console.WriteLine("[Model] Loaded {0} ({1} vertices, {2} faces)",
                    Path.GetFileName(SourcePath), vertices.Count, indexCount / 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Model] Failed to load {0}: {1}", SourcePath, ex.Message);
                Loaded = false;
            }
        }

        private void LoadOBJ(string path)
        {
            using (var sr = new StreamReader(path))
            {
                string line;
                var tempNormals = new List<Vector3>();

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("v "))
                    {
                        var p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        vertices.Add(new Vector3(
                            float.Parse(p[1], CultureInfo.InvariantCulture),
                            float.Parse(p[2], CultureInfo.InvariantCulture),
                            float.Parse(p[3], CultureInfo.InvariantCulture)));
                    }
                    else if (line.StartsWith("vn "))
                    {
                        var p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        tempNormals.Add(new Vector3(
                            float.Parse(p[1], CultureInfo.InvariantCulture),
                            float.Parse(p[2], CultureInfo.InvariantCulture),
                            float.Parse(p[3], CultureInfo.InvariantCulture)));
                    }
                    else if (line.StartsWith("f "))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < parts.Length - 2; i++)
                        {
                            ParseFace(parts[1], tempNormals);
                            ParseFace(parts[i + 1], tempNormals);
                            ParseFace(parts[i + 2], tempNormals);
                        }
                    }
                }
            }
        }

        private void ParseFace(string token, List<Vector3> tempNormals)
        {
            var parts = token.Split('/');
            int vi = int.Parse(parts[0], CultureInfo.InvariantCulture) - 1;
            indices.Add(vi);

            if (parts.Length >= 3 && parts[2] != "")
            {
                int ni = int.Parse(parts[2], CultureInfo.InvariantCulture) - 1;
                while (normals.Count <= vi) normals.Add(Vector3.Zero);
                normals[vi] = tempNormals[ni];
            }
        }

        private void ComputeNormals()
        {
            normals = new List<Vector3>(new Vector3[vertices.Count]);
            for (int i = 0; i < indices.Count; i += 3)
            {
                int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
                Vector3 v0 = vertices[i0];
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 n = Vector3.Cross(v1 - v0, v2 - v0);
                n.Normalize();
                normals[i0] += n;
                normals[i1] += n;
                normals[i2] += n;
            }
            for (int i = 0; i < normals.Count; i++)
            {
                if (normals[i].LengthSquared > 0)
                    normals[i].Normalize();
            }
        }

        private void ComputeBounds()
        {
            if (vertices.Count == 0) return;
            minBound = maxBound = vertices[0];
            foreach (var v in vertices)
            {
                minBound = Vector3.ComponentMin(minBound, v);
                maxBound = Vector3.ComponentMax(maxBound, v);
            }
            center = (minBound + maxBound) * 0.5f;
            size = maxBound - minBound;
        }

        private void UploadToGPU()
        {
            int[] buffers = new int[3];
            GL.GenBuffers(3, buffers);
            vbo = buffers[0];
            nbo = buffers[1];
            ebo = buffers[2];

            // Vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                (IntPtr)(vertices.Count * Vector3.SizeInBytes),
                vertices.ToArray(), BufferUsageHint.StaticDraw);

            // Normals
            GL.BindBuffer(BufferTarget.ArrayBuffer, nbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                (IntPtr)(normals.Count * Vector3.SizeInBytes),
                normals.ToArray(), BufferUsageHint.StaticDraw);

            // Indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                (IntPtr)(indices.Count * sizeof(int)),
                indices.ToArray(), BufferUsageHint.StaticDraw);

            indexCount = indices.Count;
        }

        public void Render(Vector3 position, float scale, float rotX, float rotY, float rotZ)
        {
            if (!Loaded) return;

            GL.PushMatrix();
            GL.Translate(position);
            if (rotX != 0f) GL.Rotate(rotX, 1f, 0f, 0f);
            if (rotY != 0f) GL.Rotate(rotY, 0f, 1f, 0f);
            if (rotZ != 0f) GL.Rotate(rotZ, 0f, 0f, 1f);
            GL.Scale(scale, scale, scale);
            GL.Translate(-center);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, nbo);
            GL.NormalPointer(NormalPointerType.Float, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.DrawElements(PrimitiveType.Triangles, indexCount,
                DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);

            GL.PopMatrix();
        }

        public void Dispose()
        {
            if (vbo != 0) GL.DeleteBuffer(vbo);
            if (nbo != 0) GL.DeleteBuffer(nbo);
            if (ebo != 0) GL.DeleteBuffer(ebo);
        }
    }
}