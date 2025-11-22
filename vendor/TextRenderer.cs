using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace skystride.vendor
{
    internal class TextRenderer : IDisposable
    {
        private int _lastTexture = 0;
        private int _lastW = 0;
        private int _lastH = 0;

        // Shared singleton to support static access
        private static readonly TextRenderer Shared = new TextRenderer();

        // Static convenience to allow TextRenderer.RenderText(...)
        public static void RenderText(string text, float x, float y, Color color, int windowWidth, int windowHeight, float fontSize = 16f)
        {
            Shared.DrawTextInternal(text, x, y, color, windowWidth, windowHeight, "Consolas", fontSize);
        }

        public static void RenderText(string text, float x, float y, Color color, int windowWidth, int windowHeight, string fontName, float fontSize)
        {
            Shared.DrawTextInternal(text, x, y, color, windowWidth, windowHeight, fontName, fontSize);
        }

        public void DrawTextInternal(string text, float x, float y, Color color, int windowWidth, int windowHeight, string fontName = "Consolas", float fontSize = 16f)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Create bitmap via GDI+
            using (var bmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bmp))
            using (var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                // measure
                var size = g.MeasureString(text, font);
                int w = Math.Max(1, (int)Math.Ceiling(size.Width));
                int h = Math.Max(1, (int)Math.Ceiling(size.Height));

                using (var realBmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                using (var realG = Graphics.FromImage(realBmp))
                {
                    realG.Clear(Color.Transparent);
                    realG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    using (var brush = new SolidBrush(color))
                    {
                        realG.DrawString(text, font, brush, 0f, 0f);
                    }

                    UploadTexture(realBmp, w, h);
                }

                // Set up2D orthographic projection
                GL.MatrixMode(MatrixMode.Projection);
                GL.PushMatrix();
                GL.LoadIdentity();
                GL.Ortho(0, windowWidth, windowHeight, 0, -1, 1);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.PushMatrix();
                GL.LoadIdentity();

                // Save state
                bool hadDepthTest = GL.IsEnabled(EnableCap.DepthTest);
                bool hadFog = GL.IsEnabled(EnableCap.Fog);

                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, _lastTexture);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Disable(EnableCap.DepthTest);
                GL.DepthMask(false); // don't write to depth buffer for overlay
                if (hadFog) GL.Disable(EnableCap.Fog); // avoid fogging text

                float fx0 = x;
                float fy0 = y;
                float fx1 = x + _lastW;
                float fy1 = y + _lastH;

                GL.Color4(1f, 1f, 1f, 1f);
                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0f, 0f); GL.Vertex2(fx0, fy0);
                GL.TexCoord2(1f, 0f); GL.Vertex2(fx1, fy0);
                GL.TexCoord2(1f, 1f); GL.Vertex2(fx1, fy1);
                GL.TexCoord2(0f, 1f); GL.Vertex2(fx0, fy1);
                GL.End();

                // Restore state
                if (hadFog) GL.Enable(EnableCap.Fog);
                GL.DepthMask(true);
                if (hadDepthTest) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.Texture2D);

                GL.MatrixMode(MatrixMode.Modelview);
                GL.PopMatrix();
                GL.MatrixMode(MatrixMode.Projection);
                GL.PopMatrix();
                GL.MatrixMode(MatrixMode.Modelview);
            }
        }

        private void UploadTexture(Bitmap bmp, int w, int h)
        {
            // Delete previous texture
            if (_lastTexture != 0)
            {
                try { GL.DeleteTexture(_lastTexture); } catch { }
                _lastTexture = 0;
            }

            int handle;
            GL.GenTextures(1, out handle);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            _lastTexture = handle;
            _lastW = w;
            _lastH = h;
        }

        public void Dispose()
        {
            if (_lastTexture != 0)
            {
                try { GL.DeleteTexture(_lastTexture); } catch { }
                _lastTexture = 0;
            }
        }
    }
}
