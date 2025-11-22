using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using skystride.scenes;

namespace skystride.objects
{
    internal class Skybox : ISceneEntity, IDisposable
    {
        private Vector3 _position;
        private float _size;
        private int _textureHandle;
        private bool _textureReady;

        public Skybox(string texturePathRelative = "assets/textures/skybox.jpg", float size = 200f)
        {
            _size = size <= 0f ? 200f : size;
            LoadTexture(texturePathRelative);
        }

        public void SetPosition(Vector3 p) { _position = p; }
        public Vector3 GetPosition() { return _position; }

        private void LoadTexture(string relativePath)
        {
            _textureHandle = 0;
            _textureReady = false;
            try
            {
                string fullPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, relativePath.TrimStart('/', '\\'));

                if (!File.Exists(fullPath)) return;

                int handle;
                GL.GenTextures(1, out handle);
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

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                _textureHandle = handle;
                _textureReady = true;
            }
            catch
            {
                _textureHandle = 0;
                _textureReady = false;
            }
        }

        public void Render()
        {
            bool hadDepthTest = GL.IsEnabled(EnableCap.DepthTest);
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);

            if (_textureReady)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, _textureHandle);
                GL.Color3(1f, 1f, 1f);
            }

            float h = _size * 0.5f;
            float px = _position.X, py = _position.Y, pz = _position.Z;

            Vector3
            v000 = new Vector3(px - h, py - h, pz - h),
            v001 = new Vector3(px - h, py - h, pz + h),
            v010 = new Vector3(px - h, py + h, pz - h),
            v011 = new Vector3(px - h, py + h, pz + h),
            v100 = new Vector3(px + h, py - h, pz - h),
            v101 = new Vector3(px + h, py - h, pz + h),
            v110 = new Vector3(px + h, py + h, pz - h),
            v111 = new Vector3(px + h, py + h, pz + h);

            GL.Begin(PrimitiveType.Quads);

            // +Z face (front) - UV so that image top is +Y
            if (_textureReady) { GL.TexCoord2(0f, 0f); } GL.Vertex3(v011); // top-left
            if (_textureReady) { GL.TexCoord2(1f, 0f); } GL.Vertex3(v111); // top-right
            if (_textureReady) { GL.TexCoord2(1f, 1f); } GL.Vertex3(v101); // bottom-right
            if (_textureReady) { GL.TexCoord2(0f, 1f); } GL.Vertex3(v001); // bottom-left

            // -Z face (back) - keep same rotation (top is +Y, left is +X)
            if (_textureReady) { GL.TexCoord2(0f, 0f); } GL.Vertex3(v110); // top-left (+X)
            if (_textureReady) { GL.TexCoord2(1f, 0f); } GL.Vertex3(v010); // top-right (-X)
            if (_textureReady) { GL.TexCoord2(1f, 1f); } GL.Vertex3(v000); // bottom-right
            if (_textureReady) { GL.TexCoord2(0f, 1f); } GL.Vertex3(v100); // bottom-left

            // +X face (right) - keep same rotation (top is +Y, left is +Z)
            if (_textureReady) { GL.TexCoord2(0f, 0f); } GL.Vertex3(v111); // top-left (+Z)
            if (_textureReady) { GL.TexCoord2(1f, 0f); } GL.Vertex3(v110); // top-right (-Z)
            if (_textureReady) { GL.TexCoord2(1f, 1f); } GL.Vertex3(v100); // bottom-right
            if (_textureReady) { GL.TexCoord2(0f, 1f); } GL.Vertex3(v101); // bottom-left

            // -X face (left) - keep same rotation (top is +Y, left is -Z)
            if (_textureReady) { GL.TexCoord2(0f, 0f); } GL.Vertex3(v010); // top-left (-Z)
            if (_textureReady) { GL.TexCoord2(1f, 0f); } GL.Vertex3(v011); // top-right (+Z)
            if (_textureReady) { GL.TexCoord2(1f, 1f); } GL.Vertex3(v001); // bottom-right
            if (_textureReady) { GL.TexCoord2(0f, 1f); } GL.Vertex3(v000); // bottom-left

            // top and bottom faces intentionally not drawn
            GL.End();

            if (_textureReady)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.Texture2D);
            }

            // restore depth state
            GL.DepthMask(true);
            if (hadDepthTest) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
        }

        public void Dispose()
        {
            if (_textureHandle != 0)
            {
                try { GL.DeleteTexture(_textureHandle); } catch { }
                _textureHandle = 0;
            }
        }
    }
}
