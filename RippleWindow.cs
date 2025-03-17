using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Windows.Controls;

namespace RIKA_TIMER
{
    public class RippleWindow : Window
    {
        private const double DEFAULT_SIZE = 100;
        private const double INITIAL_SCALE = 0;
        private const double DURATION = 2.0;
        private readonly Ellipse _ellipse;
        private readonly ScaleTransform _scaleTransform;

        public RippleWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;
            IsHitTestVisible = false;
            var canvas = new Canvas();

            _ellipse = new Ellipse
            {
                StrokeThickness = 3 * DEFAULT_SIZE / 50,
                Fill = Brushes.Transparent,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _scaleTransform = new ScaleTransform()
            };

            canvas.Children.Add(_ellipse);
            Content = canvas;
            SetSize(DEFAULT_SIZE);
        }

        private void SetSize(double size)
        {
            Width = size * 2;
            Height = size * 2;
            _ellipse.Width = size * 2;
            _ellipse.Height = size * 2;
        }

        public void ShowEffect(Point screenPos, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    Left = screenPos.X - DEFAULT_SIZE;
                    Top = screenPos.Y - DEFAULT_SIZE;
                    _ellipse.Stroke = color;
                    _ellipse.Opacity = 1.0;
                    _scaleTransform.ScaleX = INITIAL_SCALE;
                    _scaleTransform.ScaleY = INITIAL_SCALE;

                    var scaleAnim = new DoubleAnimation(INITIAL_SCALE, 1.0, TimeSpan.FromSeconds(DURATION))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };

                    var opacityAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(DURATION))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };

                    _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                    _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
                    _ellipse.BeginAnimation(OpacityProperty, opacityAnim);

                    Show();

                    Task.Delay((int)(DURATION * 1000)).ContinueWith(_ => Dispatcher.Invoke(() =>
                    {
                        Hide();
                        RippleManager.ReturnToPool(this);
                    }));
                }

                catch
                {
                    Hide();
                    RippleManager.ReturnToPool(this);
                }
            });
        }
    }
}
