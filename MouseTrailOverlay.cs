using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace RIKA_TIMER
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using System.Runtime.InteropServices;

    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using System.Runtime.InteropServices;
    using System.Windows.Interop;

    public class MouseTrailOverlay
    {
        private Window _overlayWindow;
        private Canvas _canvas;
        private Point _previousMousePos;
        private DispatcherTimer _renderTimer;
        private const double FadeFactor = 0.83;
        private const double VerticalShift = 6;
        private const int TrailWidth = 4;

        public void Start()
        {
            _overlayWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                IsHitTestVisible = false,
                ShowInTaskbar = false,
                Left = 0,
                Top = 0,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                Focusable = false,  // Окно не может получать фокус
                ShowActivated = false  // Окно не активируется при показе
            };

            _canvas = new Canvas
            {
                Width = _overlayWindow.Width,
                Height = _overlayWindow.Height,
                Background = Brushes.Transparent,
                IsHitTestVisible = false // Важно: канвас не блокирует клики
            };
            _overlayWindow.Content = _canvas;

            _renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.0 / 100.0) // Увеличили частоту до 100 FPS
            };
            _renderTimer.Tick += RenderFrame;
            _renderTimer.Start();

            // Инициализация позиций мыши
            var pos = GetCurrentMousePosition();

            _previousMousePos = pos;

            _overlayWindow.Show();
            SetWindowExTransparent(_overlayWindow);
        }

        private Point GetCurrentMousePosition()
        {
            var win32MousePos = Win32.GetCursorPosition();
            return new Point(win32MousePos.X, win32MousePos.Y);
        }

        private void RenderFrame(object sender, EventArgs e)
        {
            var currentPos = GetCurrentMousePosition();

            foreach (var line in _canvas.Children.OfType<Line>().ToList())
            {
                line.Y1 -= VerticalShift;
                line.Y2 -= VerticalShift;

                if (line.Stroke is SolidColorBrush brush)
                {
                    brush.Opacity *= FadeFactor;
                    if (brush.Opacity < 0.05)
                    {
                        _canvas.Children.Remove(line);
                    }
                }
            }

            _previousMousePos.Y -= VerticalShift;

            int ms = DateTime.UtcNow.Millisecond;
            float cycle = ms / 1000f;
            Color clr = GetRainbowColor(cycle);

            var trailLine = new Line
            {
                X1 = currentPos.X,
                Y1 = currentPos.Y,
                X2 = _previousMousePos.X,
                Y2 = _previousMousePos.Y,
                Stroke = new SolidColorBrush(clr),
                StrokeThickness = TrailWidth,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false // Важно: линия не блокирует клики
            };
            _canvas.Children.Add(trailLine);

            _previousMousePos = currentPos;
        }

        public void Stop()
        {
            _renderTimer?.Stop();
            _overlayWindow?.Close();
        }

        private static void SetWindowExTransparent(Window what)
        {
            var hwnd = new WindowInteropHelper(what).Handle;
            Win32.SetWindowExTransparent(hwnd);
        }

        private static class Win32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;
            }

            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out POINT lpPoint);

            public static POINT GetCursorPosition()
            {
                GetCursorPos(out POINT lpPoint);
                return lpPoint;
            }

            // И добавьте в класс Win32:
            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll")]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            public static void SetWindowExTransparent(IntPtr hwnd)
            {
                const int GWL_EXSTYLE = -20;
                const int WS_EX_TRANSPARENT = 0x00000020;
                const int WS_EX_LAYERED = 0x00080000;
                const int WS_EX_NOACTIVATE = 0x08000000;

                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE);
            }
        }

        public static Color GetRainbowColor(float x)
        {
            // Нормализуем x в диапазон [0, 1), чтобы он был зацикленным
            x = x % 1.0f;
            if (x < 0) x += 1.0f; // Обрабатываем отрицательные значения

            // Разбиваем диапазон на 6 секторов (каждый соответствует части радуги)
            float sectorSize = 1.0f / 6.0f;
            int sector = (int)(x / sectorSize);
            float positionInSector = (x % sectorSize) / sectorSize;

            // Генерируем RGB-значения в зависимости от сектора
            float r = 0, g = 0, b = 0;
            switch (sector)
            {
                case 0: // Красный -> Жёлтый
                    r = 1.0f;
                    g = positionInSector;
                    b = 0.0f;
                    break;
                case 1: // Жёлтый -> Зелёный
                    r = 1.0f - positionInSector;
                    g = 1.0f;
                    b = 0.0f;
                    break;
                case 2: // Зелёный -> Голубой
                    r = 0.0f;
                    g = 1.0f;
                    b = positionInSector;
                    break;
                case 3: // Голубой -> Синий
                    r = 0.0f;
                    g = 1.0f - positionInSector;
                    b = 1.0f;
                    break;
                case 4: // Синий -> Пурпурный
                    r = positionInSector;
                    g = 0.0f;
                    b = 1.0f;
                    break;
                case 5: // Пурпурный -> Красный
                    r = 1.0f;
                    g = 0.0f;
                    b = 1.0f - positionInSector;
                    break;
            }

            // Преобразуем значения [0, 1] в байты [0, 255]
            return Color.FromArgb(
                255,
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255)
            );
        }
    }
}
