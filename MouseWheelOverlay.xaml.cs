using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RIKA_TIMER
{
    public partial class MouseWheelOverlay : Window
    {
        private readonly DispatcherTimer _cleanupTimer;
        private DateTime _lastWheelTime;
        private Point _lastMousePos;
        private bool _lastDirectionUp;
        private GlobalMouseHook _hook;
        private readonly List<UIElement> _arrows = new();
        private double _currentOffsetY;
        private double _interval = 0.20;

        public MouseWheelOverlay()
        {
            InitializeComponent();
            InitializeHook();
            SetDpiScale();
            SetWindowSize();

            _cleanupTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_interval) };
            _cleanupTimer.Tick += (s, e) => ClearArrows();

            Loaded += (s, e) => SetWindowSize();

            // Добавляем обработчик движения мыши
            CompositionTarget.Rendering += (s, e) => UpdateArrowsPosition();
        }

        private void InitializeHook()
        {
            _hook = new GlobalMouseHook();
            _hook.MouseWheel += OnMouseWheel;
            _hook.Start();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateMousePosition();

                bool currentDirectionUp = e.Delta > 0;
                Debug.WriteLine($"Direction: {(currentDirectionUp ? "Up" : "Down")}");

                // Сброс смещения при изменении направления
                if (currentDirectionUp != _lastDirectionUp ||
                   (DateTime.Now - _lastWheelTime).TotalSeconds > 0.3)
                {
                    _currentOffsetY = 0;
                    ClearArrows();
                }

                AddArrow(currentDirectionUp);

                _lastDirectionUp = currentDirectionUp;
                _lastWheelTime = DateTime.Now;
                _cleanupTimer.Stop();
                _cleanupTimer.Start();
            });
        }

        private void UpdateMousePosition()
        {
            Win32.GetCursorPos(out var point);
            var source = PresentationSource.FromVisual(this);
            _lastMousePos = source?.CompositionTarget?.TransformFromDevice.Transform(
                new Point(point.X, point.Y)) ?? new Point(point.X, point.Y);
        }

        private void SetDpiScale()
        {
            var dpi = VisualTreeHelper.GetDpi(this);
            var scale = new ScaleTransform(1 / dpi.DpiScaleX, 1 / dpi.DpiScaleY);
            ScreenCanvas.LayoutTransform = scale;
        }

        private void SetWindowSize()
        {
            var screen = WpfScreen.GetScreenFrom(this);
            Width = screen.PhysicalBounds.Width;
            Height = screen.PhysicalBounds.Height;
            Left = screen.PhysicalBounds.Left;
            Top = screen.PhysicalBounds.Top;
        }

        private void AddArrow(bool isUp)
        {
            const double offsetStep = 12.5;
            double yOffset = isUp ? _currentOffsetY - offsetStep : _currentOffsetY + offsetStep;

            var arrow = new Path
            {
                Data = Geometry.Parse(isUp ? "M 0,10 L 10,0 20,10 10,7 Z" : "M 0,0 L 10,10 20,0 10,3 Z"),
                Fill = Brushes.LimeGreen,
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 1,
                Width = 20,
                Height = 20,
                RenderTransform = new TranslateTransform(-10, -10 + yOffset)
            };

            _currentOffsetY = yOffset;
            UpdateArrowsPosition();
            ScreenCanvas.Children.Add(arrow);
            _arrows.Add(arrow);
        }

        // Обновляем позиции всех стрелок
        private void UpdateArrowsPosition()
        {
            UpdateMousePosition();
            foreach (UIElement arrow in _arrows)
            {
                Canvas.SetLeft(arrow, _lastMousePos.X);
                Canvas.SetTop(arrow, _lastMousePos.Y);
            }
        }

        private void ClearArrows()
        {
            _currentOffsetY = 0;
            foreach (var arrow in _arrows)
            {
                ScreenCanvas.Children.Remove(arrow);
            }
            _arrows.Clear();
        }
    }
}