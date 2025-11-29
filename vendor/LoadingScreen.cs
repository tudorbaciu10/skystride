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

            // Draw dark background overlay
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(0.0f, 0.0f, 0.0f, 0.95f); // Almost black
            GL.Vertex2(0, 0);
            GL.Vertex2(windowWidth, 0);
            GL.Vertex2(windowWidth, windowHeight);
            GL.Vertex2(0, windowHeight);
            GL.End();

            // Draw loading text
            int textY = windowHeight / 2 - 60;
            TextRenderer.RenderText(_loadingText, windowWidth / 2 - 100, textY, Color.White, windowWidth, windowHeight, 24f);

            // Draw progress bar (Half-Life style with blocks)
            int barWidth = 400;
            int barHeight = 30;
            int barX = (windowWidth - barWidth) / 2;
            int barY = windowHeight / 2;

            // Draw background/border of progress bar
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(0.3f, 0.3f, 0.3f, 1.0f); // Dark gray background
            GL.Vertex2(barX - 2, barY - 2);
            GL.Vertex2(barX + barWidth + 2, barY - 2);
            GL.Vertex2(barX + barWidth + 2, barY + barHeight + 2);
            GL.Vertex2(barX - 2, barY + barHeight + 2);
            GL.End();

            // Draw inner black area
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Vertex2(barX, barY);
            GL.Vertex2(barX + barWidth, barY);
            GL.Vertex2(barX + barWidth, barY + barHeight);
            GL.Vertex2(barX, barY + barHeight);
            GL.End();

            // Draw progress blocks (HL style)
            int blockCount = 30;
            int blockWidth = (barWidth - (blockCount - 1) * 2) / blockCount; // 2px spacing
            int filledBlocks = (int)(_progress * blockCount);

            for (int i = 0; i < filledBlocks; i++)
            {
                int blockX = barX + i * (blockWidth + 2);
                
                GL.Begin(PrimitiveType.Quads);
                GL.Color4(0.9f, 0.9f, 0.9f, 1.0f); // Light gray/white blocks
                GL.Vertex2(blockX, barY + 2);
                GL.Vertex2(blockX + blockWidth, barY + 2);
                GL.Vertex2(blockX + blockWidth, barY + barHeight - 2);
                GL.Vertex2(blockX, barY + barHeight - 2);
                GL.End();
            }

            // Draw percentage text below bar
            int percentage = (int)(_progress * 100);
            string percentText = $"{percentage}%";
            TextRenderer.RenderText(percentText, windowWidth / 2 - 20, barY + barHeight + 20, Color.LightGray, windowWidth, windowHeight, 16f);

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
