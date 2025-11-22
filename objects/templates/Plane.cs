using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using skystride.scenes;
using System;
using System.Drawing.Imaging;
using System.IO;

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

        // Rotation (Euler angles in degrees: X=pitch, Y=yaw, Z=roll)
        private Vector3 rotationEulerDeg = Vector3.Zero;

        // Texture fields
        private int textureHandle; //0 => no texture
        private bool textureEnabled; // if true and textureHandle !=0, render textured
        private float texScaleU = 1f; // tiling along local X or width on top/bottom
        private float texScaleV = 1f; // tiling along local Z or depth on top/bottom

        public Plane() : this(new Vector3(0f, 0f, 0f), 1f, 1f, 1f, Color.LightGray, new Vector3(0f, 1f, 0f)) { }

        public Plane(Vector3 position, float width, float depth, Color color, Vector3 normal)
         : this(position, width, depth, 0f, color, normal) { }

        public Plane(Vector3 position, float width, float depth, float height, Color color, Vector3 normal)
        {
            this.position = position;
            this.width = width <= 0f ? 1f : width;
            this.depth = depth <= 0f ? 1f : depth;
            this.height = height < 0f ? 0f : height; // negative height coerced to0 (flat)
            this.color = color;
            this.normal = normal.LengthSquared > 0f ? Vector3.Normalize(normal) : new Vector3(0f, 1f, 0f);
            this.textureHandle = 0;
            this.textureEnabled = false;
        }

        public Vector3 GetPosition() { return this.position; }
        public Vector3 GetSize() { return new Vector3(this.width, this.height, this.depth); }

        public void SetPosition(Vector3 pos) { this.position = pos; }
        public void SetSize(float width, float depth)
        {
            if (width > 0f) this.width = width;
            if (depth > 0f) this.depth = depth;
        }
        public void SetSize(float width, float depth, float height)
        {
            SetSize(width, depth);
            SetHeight(height);
        }
        public void SetHeight(float h)
        {
            if (h >= 0f) this.height = h; // allow0 -> flat
        }
        public void SetColor(Color c) { this.color = c; }
        public void SetNormal(Vector3 n)
        {
            if (n.LengthSquared > 0f) this.normal = Vector3.Normalize(n);
        }

        public void SetRotation(Vector3 eulerDegrees)
        {
            this.rotationEulerDeg = eulerDegrees;
        }
        public void SetRotation(float xDeg, float yDeg, float zDeg)
        {
            this.rotationEulerDeg = new Vector3(xDeg, yDeg, zDeg);
        }
        public Vector3 GetRotation()
        {
            return this.rotationEulerDeg;
        }
        public void Rotate(float dxDeg, float dyDeg, float dzDeg)
        {
            this.rotationEulerDeg += new Vector3(dxDeg, dyDeg, dzDeg);
        }

        // Texture API
        public void SetTexture(string path)
        {
            if (this.textureHandle != 0)
            {
                try { GL.DeleteTexture(this.textureHandle); } catch { /* ignore */ }
                this.textureHandle = 0;
            }

            string baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string candidate = path ?? string.Empty;
            string fullPath;
            if (Path.IsPathRooted(candidate))
                fullPath = candidate;
            else
            {
                var trimChars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
                fullPath = Path.Combine(baseDir, candidate.TrimStart(trimChars));
            }

            if (!File.Exists(fullPath))
            {
                this.textureEnabled = false;
                return;
            }

            try
            {
                int handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, handle);

                using (var bmp = new Bitmap(fullPath))
                {
                    var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                        data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                        PixelType.UnsignedByte, data.Scan0);
                    bmp.UnlockBits(data);
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                GL.BindTexture(TextureTarget.Texture2D, 0);

                this.textureHandle = handle;
                this.textureEnabled = true;
            }
            catch
            {
                this.textureHandle = 0;
                this.textureEnabled = false;
            }
        }

        public void SetTextureEnabled(bool enabled)
        {
            this.textureEnabled = enabled && this.textureHandle != 0;
        }

        public void SetTextureScale(float u, float v)
        {
            this.texScaleU = u <= 0f ? 1f : u;
            this.texScaleV = v <= 0f ? 1f : v;
        }

        public void Render()
        {
            float hw = this.width * 0.5f; // Half width
            float hd = this.depth * 0.5f; // Half depth
            float hh = this.height * 0.5f; // Half height/thickness
            float px = position.X, py = position.Y, pz = position.Z;

            GL.PushMatrix();
            GL.Translate(px, py, pz);
            if (rotationEulerDeg.Y != 0f) GL.Rotate(rotationEulerDeg.Y, 0f, 1f, 0f); // Yaw
            if (rotationEulerDeg.X != 0f) GL.Rotate(rotationEulerDeg.X, 1f, 0f, 0f); // Pitch
            if (rotationEulerDeg.Z != 0f) GL.Rotate(rotationEulerDeg.Z, 0f, 0f, 1f); // Roll

            Vector3 top00 = new Vector3(-hw, +hh, -hd);
            Vector3 top01 = new Vector3(-hw, +hh, +hd);
            Vector3 top10 = new Vector3(+hw, +hh, -hd);
            Vector3 top11 = new Vector3(+hw, +hh, +hd);

            bool useTexture = this.textureEnabled && this.textureHandle != 0;
            if (useTexture)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, this.textureHandle);
                GL.Color3(1f, 1f, 1f); // avoid tint
            }
            else
            {
                GL.Color3(this.color);
            }

            GL.Begin(PrimitiveType.Quads);

            if (this.height <= 0f)
            {
                GL.Normal3(this.normal);
                if (useTexture)
                {
                    GL.TexCoord2(0f, 0f); GL.Vertex3(top00);
                    GL.TexCoord2(this.texScaleU, 0f); GL.Vertex3(top10);
                    GL.TexCoord2(this.texScaleU, this.texScaleV); GL.Vertex3(top11);
                    GL.TexCoord2(0f, this.texScaleV); GL.Vertex3(top01);
                }
                else
                {
                    GL.Vertex3(top00);
                    GL.Vertex3(top10);
                    GL.Vertex3(top11);
                    GL.Vertex3(top01);
                }
            }
            else
            {
                // Compute bottom face vertices (local-space)
                Vector3 bottom00 = new Vector3(-hw, -hh, -hd);
                Vector3 bottom01 = new Vector3(-hw, -hh, +hd);
                Vector3 bottom10 = new Vector3(+hw, -hh, -hd);
                Vector3 bottom11 = new Vector3(+hw, -hh, +hd);

                // Top face
                GL.Normal3(this.normal);
                if (useTexture)
                {
                    GL.TexCoord2(0f, 0f); GL.Vertex3(top00);
                    GL.TexCoord2(this.texScaleU, 0f); GL.Vertex3(top10);
                    GL.TexCoord2(this.texScaleU, this.texScaleV); GL.Vertex3(top11);
                    GL.TexCoord2(0f, this.texScaleV); GL.Vertex3(top01);
                }
                else
                {
                    GL.Vertex3(top00);
                    GL.Vertex3(top10);
                    GL.Vertex3(top11);
                    GL.Vertex3(top01);
                }

                // Bottom face (inverse normal)
                GL.Normal3(-this.normal);
                if (useTexture)
                {
                    GL.TexCoord2(this.texScaleU, 0f); GL.Vertex3(bottom10);
                    GL.TexCoord2(0f, 0f); GL.Vertex3(bottom00);
                    GL.TexCoord2(0f, this.texScaleV); GL.Vertex3(bottom01);
                    GL.TexCoord2(this.texScaleU, this.texScaleV); GL.Vertex3(bottom11);
                }
                else
                {
                    GL.Vertex3(bottom10);
                    GL.Vertex3(bottom00);
                    GL.Vertex3(bottom01);
                    GL.Vertex3(bottom11);
                }

                // +X side
                GL.Normal3(1f, 0f, 0f);
                if (useTexture)
                {
                    // u -> Z (depth), v -> Y (height)
                    GL.TexCoord2(0f, this.texScaleV); GL.Vertex3(top10);
                    GL.TexCoord2(0f, 0f); GL.Vertex3(bottom10);
                    GL.TexCoord2(this.texScaleU, 0f); GL.Vertex3(bottom11);
                    GL.TexCoord2(this.texScaleU, this.texScaleV); GL.Vertex3(top11);
                }
                else
                {
                    GL.Vertex3(top10);
                    GL.Vertex3(bottom10);
                    GL.Vertex3(bottom11);
                    GL.Vertex3(top11);
                }
                // -X side
                GL.Normal3(-1f, 0f, 0f);
                if (useTexture)
                {
                    GL.TexCoord2(0f, 0f); GL.Vertex3(bottom00);
                    GL.TexCoord2(0f, this.texScaleV); GL.Vertex3(top00);
                    GL.TexCoord2(this.texScaleU, this.texScaleV); GL.Vertex3(top01);
                    GL.TexCoord2(this.texScaleU, 0f); GL.Vertex3(bottom01);
                }
                else
                {
                    GL.Vertex3(bottom00);
                    GL.Vertex3(top00);
                    GL.Vertex3(top01);
                    GL.Vertex3(bottom01);
                }
                // +Z side
                GL.Normal3(0f, 0f, 1f);
                if (useTexture)
                {
                    // u -> X (width), v -> Y (height)
                    GL.TexCoord2(0f, this.texScaleV); GL.Vertex3(top01);
                    GL.TexCoord2(this.texScaleU, this.texScaleV); GL.Vertex3(top11);
                    GL.TexCoord2(this.texScaleU, 0f); GL.Vertex3(bottom11);
                    GL.TexCoord2(0f, 0f); GL.Vertex3(bottom01);
                }
                else
                {
                    GL.Vertex3(top01);
                    GL.Vertex3(top11);
                    GL.Vertex3(bottom11);
                    GL.Vertex3(bottom01);
                }
                // -Z side
                GL.Normal3(0f, 0f, -1f);
                if (useTexture)
                {
                    GL.TexCoord2(this.texScaleU, this.texScaleV); GL.Vertex3(top10);
                    GL.TexCoord2(0f, this.texScaleV); GL.Vertex3(top00);
                    GL.TexCoord2(0f, 0f); GL.Vertex3(bottom00);
                    GL.TexCoord2(this.texScaleU, 0f); GL.Vertex3(bottom10);
                }
                else
                {
                    GL.Vertex3(top10);
                    GL.Vertex3(top00);
                    GL.Vertex3(bottom00);
                    GL.Vertex3(bottom10);
                }
            }

            GL.End();

            if (useTexture)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.Texture2D);
            }

            GL.PopMatrix();
        }
    }
}
