using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RIKA_TIMER
{
    public static class RippleManager
    {
        private const int POOL_SIZE = 10;
        private static readonly Queue<RippleWindow> _windowPool = new Queue<RippleWindow>();
        private static GlobalMouseHook _hook;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X; public int Y; }

        public static void Initialize()
        {
            for (int i = 0; i < POOL_SIZE; i++)
                _windowPool.Enqueue(new RippleWindow());

            _hook = new GlobalMouseHook();
            _hook.MouseDown += OnMouseDown;
            _hook.Start();
        }

        public static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_windowPool.Count == 0) return;

            GetCursorPos(out POINT point);
            var window = _windowPool.Dequeue();
            window.ShowEffect(new Point(point.X, point.Y), GetColor(e.ChangedButton));
        }

        private static Brush GetColor(MouseButton button) => button switch
        {
            MouseButton.Left => new SolidColorBrush(Color.FromRgb(240, 28, 36)),
            MouseButton.Right => new SolidColorBrush(Color.FromRgb(0, 162, 232)),
            MouseButton.Middle => new SolidColorBrush(Color.FromRgb(255, 127, 39)),
            _ => Brushes.Transparent
        };

        public static void ReturnToPool(RippleWindow window) => _windowPool.Enqueue(window);
    }
}