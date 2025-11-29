using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace skystride.vendor
{
    internal class Model : IDisposable
    {
        private int vbo, nbo, tbo, ebo;
        private int indexCount;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector2> texcoords = new List<Vector2>();
        private List<int> indices = new List<int>();

        private Vector3 minBound, maxBound, center, size;
        public bool Loaded { get; private set; }
        public string SourcePath { get; private set; }
        public Vector3 Center { get { return center; } }
        public float BBoxHeight { get { return size.Y; } }

        public Vector3 BoundsSize { get { return size; } }

        private int textureHandle;
        private string detectedTexturePath;
        private bool hasTexcoords;

        private float _texScaleU = 1f;
        private float _texScaleV = 1f;

        private bool isDataLoaded = false;
        private bool isUploaded = false;
        private Bitmap textureBitmap;

        public Model(string objectPath, string objectPathTexture = "assets/textures/undefined.jpg")
        {
            SourcePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, objectPath.TrimStart('/', '\\'));
            detectedTexturePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, objectPathTexture.TrimStart('/', '\\'));

            Task.Run(() =>
            {
                try
                {
                    LoadOBJ(SourcePath);
                    if (normals.Count == 0)
                        ComputeNormals();
                    ComputeBounds();
                    if (!hasTexcoords)
                        GeneratePlanarTexcoords();

                    // Load texture data into memory but don't upload yet
                    LoadTextureBitmap();

                    isDataLoaded = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Model] Failed to load {0}: {1}", SourcePath, ex.Message);
                    Loaded = false;
                }
            });
        }

        private void LoadOBJ(string path)
        {
            var tempPositions = new List<Vector3>();
            var tempNormals = new List<Vector3>();
            var tempTex = new List<Vector2>();

            string objDir = Path.GetDirectoryName(path) ?? string.Empty;

            using (var sr = new StreamReader(path))
            {
                string line;

                var vertMap = new Dictionary<string, int>();

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("v "))
                    {
                        var p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        tempPositions.Add(new Vector3(
                            float.Parse(p[1], CultureInfo.InvariantCulture),
                            float.Parse(p[2], CultureInfo.InvariantCulture),
                            float.Parse(p[3], CultureInfo.InvariantCulture)));
                    }
                    else if (line.StartsWith("vt "))
                    {
                        var p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        float u = float.Parse(p[1], CultureInfo.InvariantCulture);
                        float v = float.Parse(p[2], CultureInfo.InvariantCulture);
                        tempTex.Add(new Vector2(u, 1f - v));
                    }
                    else if (line.StartsWith("vn "))
                    {
                        var p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        tempNormals.Add(new Vector3(
                            float.Parse(p[1], CultureInfo.InvariantCulture),
                            float.Parse(p[2], CultureInfo.InvariantCulture),
                            float.Parse(p[3], CultureInfo.InvariantCulture)));
                    }
                    else if (line.StartsWith("mtllib "))
                    {
                        var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            TryDetectTextureFromMtl(Path.Combine(objDir, parts[1]));
                        }
                    }
                    else if (line.StartsWith("f "))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < parts.Length - 2; i++)
                        {
                            AddVertexFromToken(parts[1], tempPositions, tempTex, tempNormals, vertMap);
                            AddVertexFromToken(parts[i + 1], tempPositions, tempTex, tempNormals, vertMap);
                            AddVertexFromToken(parts[i + 2], tempPositions, tempTex, tempNormals, vertMap);
                        }
                    }
                }
            }

            bool allNormalsZero = true;
            for (int i = 0; i < normals.Count; i++)
            {
                if (normals[i].LengthSquared > 1e-8f) { allNormalsZero = false; break; }
            }
            if (normals.Count == 0 || allNormalsZero)
            {
                normals.Clear();
            }
        }

        private void AddVertexFromToken(string token, List<Vector3> tempPositions, List<Vector2> tempTex, List<Vector3> tempNormals, Dictionary<string, int> vertMap)
        {
            // token format can be: v, v/vt, v//vn, v/vt/vn
            int vi = -1, ti = -1, ni = -1;
            var parts = token.Split('/');
            vi = ParseIndex(parts, 0) - 1;
            if (parts.Length > 1 && parts[1] != string.Empty)
                ti = ParseIndex(parts, 1) - 1;
            if (parts.Length > 2 && parts[2] != string.Empty)
                ni = ParseIndex(parts, 2) - 1;

            string key = vi + "/" + ti + "/" + ni;
            int outIndex;
            if (!vertMap.TryGetValue(key, out outIndex))
            {
                vertices.Add(tempPositions[vi]);
                if (ni >= 0 && ni < tempNormals.Count)
                {
                    normals.Add(tempNormals[ni]);
                }
                else
                {
                    normals.Add(Vector3.Zero);
                }

                if (ti >= 0 && ti < tempTex.Count)
                {
                    texcoords.Add(tempTex[ti]);
                    hasTexcoords = true;
                }
                else
                {
                    texcoords.Add(Vector2.Zero);
                }

                outIndex = vertices.Count - 1;
                vertMap[key] = outIndex;
            }

            indices.Add(outIndex);
        }

        private int ParseIndex(string[] parts, int i)
        {
            int val;
            if (i < parts.Length && int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
                return val;
            return 0;
        }

        private void TryDetectTextureFromMtl(string mtlPath)
        {
            try
            {
                if (!File.Exists(mtlPath)) return;
                using (var sr = new StreamReader(mtlPath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("#") || line.Length == 0) continue;
                        if (line.StartsWith("map_Kd "))
                        {
                            var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                var candidate = parts[1].Trim();
                                candidate = candidate.Trim('"');
                                string dir = Path.GetDirectoryName(mtlPath) ?? string.Empty;
                                string full = Path.Combine(dir, candidate);
                                if (File.Exists(full))
                                {
                                    detectedTexturePath = full;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Model] Failed to parse MTL {0}: {1}", mtlPath, ex.Message);
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
                if (n.LengthSquared > 0)
                {
                    n.Normalize();
                    normals[i0] += n;
                    normals[i1] += n;
                    normals[i2] += n;
                }
            }
            for (int i = 0; i < normals.Count; i++)
            {
                if (normals[i].LengthSquared > 0)
                    normals[i].Normalize();
                else
                    normals[i] = new Vector3(0, 1, 0);
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

        private void GeneratePlanarTexcoords()
        {
            // Simple XZ planar mapping scaled to bounding box
            float dx = Math.Max(1e-5f, size.X);
            float dz = Math.Max(1e-5f, size.Z);
            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                float u = (v.X - minBound.X) / dx;
                float w = (v.Z - minBound.Z) / dz;
                if (i < texcoords.Count)
                    texcoords[i] = new Vector2(u, w);
                else
                    texcoords.Add(new Vector2(u, w));
            }
        }

        private void UploadToGPU()
        {
            int[] buffers = new int[4];
            GL.GenBuffers(4, buffers);
            vbo = buffers[0];
            nbo = buffers[1];
            tbo = buffers[2];
            ebo = buffers[3];

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

            // Texcoords
            if (texcoords.Count == vertices.Count)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, tbo);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    (IntPtr)(texcoords.Count * Vector2.SizeInBytes),
                    texcoords.ToArray(), BufferUsageHint.StaticDraw);
            }

            // Indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                (IntPtr)(indices.Count * sizeof(int)),
                indices.ToArray(), BufferUsageHint.StaticDraw);

            indexCount = indices.Count;
        }

        private void LoadTextureBitmap()
        {
            string baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string fallback = Path.Combine(baseDir, "assets", "textures", "undefined.jpg");

            string pathToUse = null;
            if (!string.IsNullOrEmpty(detectedTexturePath) && File.Exists(detectedTexturePath))
                pathToUse = detectedTexturePath;
            else if (File.Exists(fallback))
                pathToUse = fallback;

            if (pathToUse != null)
            {
                try
                {
                    // Load into a temporary bitmap to avoid locking the file or threading issues with GDI+ if possible
                    // But Bitmap(path) locks the file. We can copy it.
                    using (var temp = new Bitmap(pathToUse))
                    {
                        textureBitmap = new Bitmap(temp);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Model] Failed to load texture bitmap '{0}': {1}", pathToUse, ex.Message);
                }
            }
        }

        private void UploadTexture()
        {
            if (textureBitmap == null)
            {
                Console.WriteLine("[Model] No texture found and fallback missing. Rendering untextured.");
                textureHandle = 0;
                return;
            }

            try
            {
                int handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, handle);

                var rect = new Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height);
                var data = textureBitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte, data.Scan0);
                textureBitmap.UnlockBits(data);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                textureHandle = handle;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Model] Failed to upload texture: {0}", ex.Message);
                textureHandle = 0;
            }
            finally
            {
                textureBitmap.Dispose();
                textureBitmap = null;
            }
        }

        public void SetTextureScale(float u, float v)
        {
            _texScaleU = u <= 0f ? 1f : u;
            _texScaleV = v <= 0f ? 1f : v;
        }

        public void Render(Vector3 position, float scale, float rotX, float rotY, float rotZ)
        {
            if (!isDataLoaded) return; // Still loading in background

            if (!isUploaded)
            {
                UploadToGPU();
                UploadTexture();
                isUploaded = true;
                Loaded = true;
                Console.WriteLine("[Model] Loaded {0} ({1} vertices, {2} faces){3}",
                    Path.GetFileName(SourcePath), vertices.Count, indexCount / 3, hasTexcoords ? " with texcoords" : " (generated UVs)");
            }

            if (!Loaded) return;

            GL.PushMatrix();
            GL.Translate(position);
            if (rotX != 0f) GL.Rotate(rotX, 1f, 0f, 0f);
            if (rotY != 0f) GL.Rotate(rotY, 0f, 1f, 0f);
            if (rotZ != 0f) GL.Rotate(rotZ, 0f, 0f, 1f);
            GL.Scale(scale, scale, scale);
            GL.Translate(-center);

            GL.Color3(1f, 1f, 1f);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

            bool bindTexcoords = textureHandle != 0 && texcoords.Count == vertices.Count;
            if (bindTexcoords)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, textureHandle);
                GL.EnableClientState(ArrayCap.TextureCoordArray);
                // Set texture matrix for scaling
                GL.MatrixMode(MatrixMode.Texture);
                GL.LoadIdentity();
                GL.Scale(_texScaleU, _texScaleV, 1f);
                GL.MatrixMode(MatrixMode.Modelview);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, nbo);
            GL.NormalPointer(NormalPointerType.Float, 0, IntPtr.Zero);

            if (bindTexcoords)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, tbo);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, IntPtr.Zero);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.DrawElements(PrimitiveType.Triangles, indexCount,
                DrawElementsType.UnsignedInt, IntPtr.Zero);

            if (bindTexcoords)
            {
                GL.DisableClientState(ArrayCap.TextureCoordArray);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.Texture2D);
                // Reset texture matrix
                GL.MatrixMode(MatrixMode.Texture);
                GL.LoadIdentity();
                GL.MatrixMode(MatrixMode.Modelview);
            }

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);

            GL.PopMatrix();
        }

        public void Dispose()
        {
            if (vbo != 0) GL.DeleteBuffer(vbo);
            if (nbo != 0) GL.DeleteBuffer(nbo);
            if (tbo != 0) GL.DeleteBuffer(tbo);
            if (ebo != 0) GL.DeleteBuffer(ebo);
            if (textureHandle != 0) GL.DeleteTexture(textureHandle);
            if (textureBitmap != null) textureBitmap.Dispose();
        }
    }
}