using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace skystride.vendor
{
    /// <summary>
    /// Half-Life style loading screen with progress bar
    /// </summary>
    internal class LoadingScreen
    {
        private float _progress = 0.0f;
        private bool _isVisible = false;
        private string _loadingText = "LOADING...";

        public void Show()
        {
            _isVisible = true;
            _progress = 0.0f;
        }

        public void Hide()
        {
            _isVisible = false;
            _progress = 1.0f;
        }

        public void SetProgress(float progress)
        {
            _progress = Math.Max(0.0f, Math.Min(1.0f, progress));
        }

        public void SetLoadingText(string text)
        {
            _loadingText = text ?? "LOADING...";
        }

        public bool IsVisible => _isVisible;

        public void Render(int windowWidth, int windowHeight)
        {
            if (!_isVisible) return;

            // Save current matrices
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, windowWidth, windowHeight, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Disable depth test and lighting for UI
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Texture2D);

            // HL2 Style Colors
            Color bgColor = Color.FromArgb(255, 40, 40, 40); // Dark Grey
            Color amberColor = Color.FromArgb(255, 255, 176, 0); // HL2 Amber
            Color darkAmber = Color.FromArgb(255, 74, 50, 0); // Darker Amber for empty bar

            // Draw background overlay
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(bgColor);
            GL.Vertex2(0, 0);
            GL.Vertex2(windowWidth, 0);
            GL.Vertex2(windowWidth, windowHeight);
            GL.Vertex2(0, windowHeight);
            GL.End();

            // Layout
            int centerX = windowWidth / 2;
            int centerY = windowHeight / 2;
            int barWidth = 300;
            int barHeight = 8; // Thin bar
            int barX = centerX - barWidth / 2;
            int barY = centerY + 20;

            // Draw loading text (Centered above bar)
            string text = _loadingText.ToUpper();
            float fontSize = 24f;
            // Estimate text width for centering: approx 0.6 * fontSize per char
            int textWidth = (int)(text.Length * (fontSize * 0.6f)); 
            TextRenderer.RenderText(text, centerX - textWidth / 2, barY - 40, amberColor, windowWidth, windowHeight, fontSize);

            // Draw progress bar background (Dark Amber)
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(darkAmber);
            GL.Vertex2(barX, barY);
            GL.Vertex2(barX + barWidth, barY);
            GL.Vertex2(barX + barWidth, barY + barHeight);
            GL.Vertex2(barX, barY + barHeight);
            GL.End();

            // Draw filled progress bar (Bright Amber)
            int filledWidth = (int)(barWidth * _progress);
            if (filledWidth > 0)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Color4(amberColor);
                GL.Vertex2(barX, barY);
                GL.Vertex2(barX + filledWidth, barY);
                GL.Vertex2(barX + filledWidth, barY + barHeight);
                GL.Vertex2(barX, barY + barHeight);
                GL.End();
            }

            // Restore matrices
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();

            // Re-enable depth test
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
