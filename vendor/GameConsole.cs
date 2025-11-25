using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using skystride.shaders;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace skystride.vendor
{
    internal class GameConsole
    {
        private readonly List<string> _lines = new List<string>();
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        private bool _isOpen;
        private KeyboardState _prevKeyboard;

        private Camera camera;
        private Player player;
        private Engine engine;

        public GameConsole(Camera _camera, Player _player, Engine _engine = null)
        {
            this.camera = _camera;
            this.player = _player;
            this.engine = _engine;
        }

        private const int MaxLines = 100;

        public bool IsOpen => _isOpen;

        public void Toggle()
        {
            _isOpen = !_isOpen;
        }

        public void Update(KeyboardState currentKeyboard)
        {
            if (!_isOpen)
            {
                _prevKeyboard = currentKeyboard;
                return;
            }

            // Close with Escape
            if (WasKeyPressed(currentKeyboard, Key.Escape))
            {
                _isOpen = false;
                _prevKeyboard = currentKeyboard;
                return;
            }

            // Handle character input
            foreach (Key key in _keyScanOrder)
            {
                if (WasKeyPressed(currentKeyboard, key))
                {
                    if (key == Key.BackSpace)
                    {
                        if (_inputBuffer.Length > 0)
                            _inputBuffer.Length -= 1;
                    }
                    else if (key == Key.Enter)
                    {
                        SubmitCommand(_inputBuffer.ToString());
                        _inputBuffer.Clear();
                    }
                    else
                    {
                        char c;
                        if (TryConvertKeyToChar(key, currentKeyboard.IsKeyDown(Key.ShiftLeft) || currentKeyboard.IsKeyDown(Key.ShiftRight), out c))
                        {
                            _inputBuffer.Append(c);
                        }
                    }
                }
            }

            _prevKeyboard = currentKeyboard;
        }

        private void SubmitCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
            {
                AddLine("");
                return;
            }

            AddLine("> " + cmd);

            string low = cmd.Trim().ToLowerInvariant();
            switch (low)
            {
                case "hello":
                    AddLine("Hello from console.");
                    break;
                case "help":
                    AddLine("Available commands: hello, help, clear, dev, map [name], light [1|0]");
                    break;
                case "clear":
                    _lines.Clear();
                    AddLine("Console cleared");
                    break;
                case "dev":
                    if (player.isPhysicsEnabled)
                    {
                        AddLine("Developer mode activated!");
                    }
                    else
                    {
                        AddLine("Developer mode deactivated!");
                    }

                    player.ToggleDevMode();
                    break;
                default:
                    if (low.StartsWith("map "))
                    {
                        string mapName = low.Substring("map ".Length).Trim();
                        if (engine == null)
                        {
                            AddLine("Scene switching unavailable (no engine reference).");
                        }
                        else if (string.IsNullOrEmpty(mapName))
                        {
                            AddLine("Usage: map forest | arctic");
                        }
                        else
                        {
                            bool ok = engine.ChangeScene(mapName);
                            if (ok)
                                AddLine("Loaded map: " + mapName);
                            else
                                AddLine("Unknown map. Available: forest, arctic");
                        }
                    } else if (low.StartsWith("light "))
                    {
                        string boolLight = low.Substring("light ".Length).Trim();
                        if (engine == null)
                        {
                            AddLine("Lightning switching unavailable (no engine reference).");
                        }
                        else if (string.IsNullOrEmpty(boolLight))
                        {
                            AddLine("Usage: light 1 | 0");
                        }
                        else
                        {
                            bool enable;
                            if (boolLight == "1" || boolLight == "true")
                            {
                                enable = true;
                            }
                            else if (boolLight == "0" || boolLight == "false")
                            {
                                enable = false;
                            }
                            else
                            {
                                AddLine("Invalid value. Usage: light 1 | 0");
                                return;
                            }

                            if (enable)
                            {
                                engine.lightning.Enable();
                                AddLine("Lightning enabled");
                            }
                            else
                            {
                                engine.lightning.Disable();
                                AddLine("Lightning disabled");
                            }
                        }
                    }
                    else
                    {
                        AddLine("Unknown command");
                    }
                    break;
            }
        }

        private void AddLine(string line)
        {
            _lines.Add(line);
            if (_lines.Count > MaxLines)
            {
                _lines.RemoveAt(0);
            }
        }

        public void Render(int width, int height)
        {
            if (!_isOpen) return;

            int maxLineCount = ((height - 20) / 22) - 2; // space available for stored lines (exclude input + padding)
            if (maxLineCount < 0) maxLineCount = 0;
            if (_lines.Count > maxLineCount && maxLineCount >= 0)
            {
                int removeCount = _lines.Count - maxLineCount;
                _lines.RemoveRange(0, removeCount);
            }
            int lineCount = _lines.Count + 2; // include input + padding
            int consoleHeight = lineCount * 22 + 20;

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, width, height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // blending for opacity
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // semi-transparent background quad
            GL.Disable(EnableCap.DepthTest); // ensure overlay draws on top
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(0f, 0f, 0f, 0.6f); // black with 60% opacity
            GL.Vertex2(0, 0);
            GL.Vertex2(width, 0);
            GL.Vertex2(width, consoleHeight);
            GL.Vertex2(0, consoleHeight);
            GL.End();
            GL.Enable(EnableCap.DepthTest);

            // Render text lines
            int y = 16; // padding from top
            foreach (var line in _lines)
            {
                TextRenderer.RenderText(line, 16, y, Color.LightGreen, width, height, 16f);
                y += 20;
            }
            // Render current input line
            TextRenderer.RenderText("> " + _inputBuffer.ToString(), 16, y + 4, Color.White, width, height, 16f);
            TextRenderer.RenderText("(ESC to close)", width - 220, 16, Color.Gray, width, height, 14f);

            // Restore matrices
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        private bool WasKeyPressed(KeyboardState current, Key key)
        {
            return current.IsKeyDown(key) && !_prevKeyboard.IsKeyDown(key);
        }

        private static readonly Key[] _keyScanOrder = new Key[]
        {
 // control
 Key.BackSpace, Key.Enter,
 // numbers row
 Key.Number1, Key.Number2, Key.Number3, Key.Number4, Key.Number5, Key.Number6, Key.Number7, Key.Number8, Key.Number9, Key.Number0,
 // symbols (limited)
 Key.Space, Key.Period, Key.Comma, Key.Slash, Key.Minus, Key.Plus,
 // letters
 Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J, Key.K, Key.L, Key.M,
 Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T, Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z
        };

        private bool TryConvertKeyToChar(Key key, bool shift, out char c)
        {
            c = '\0';
            if (key >= Key.A && key <= Key.Z)
            {
                int offset = key - Key.A;
                char baseChar = (char)('a' + offset);
                c = shift ? char.ToUpperInvariant(baseChar) : baseChar;
                return true;
            }
            switch (key)
            {
                case Key.Space: c = ' '; return true;
                case Key.Number0: c = shift ? ')' : '0'; return true;
                case Key.Number1: c = shift ? '!' : '1'; return true;
                case Key.Number2: c = shift ? '@' : '2'; return true;
                case Key.Number3: c = shift ? '#' : '3'; return true;
                case Key.Number4: c = shift ? '$' : '4'; return true;
                case Key.Number5: c = shift ? '%' : '5'; return true;
                case Key.Number6: c = shift ? '^' : '6'; return true;
                case Key.Number7: c = shift ? '&' : '7'; return true;
                case Key.Number8: c = shift ? '*' : '8'; return true;
                case Key.Number9: c = shift ? '(' : '9'; return true;
                case Key.Period: c = '.'; return true;
                case Key.Comma: c = ','; return true;
                case Key.Slash: c = '/'; return true;
                case Key.Minus: c = shift ? '_' : '-'; return true;
                case Key.Plus: c = shift ? '+' : '='; return true; // plus key may map to = without shift
            }
            return false;
        }
    }
}
