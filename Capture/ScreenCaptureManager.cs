
using System;
using System.Collections.Generic;
using System.Text;

namespace WinFormsApp1
{
    // ScreenCaptureManager.cs
    public class ScreenCaptureManager : IDisposable
    {
        private ScreenSelectorForm _selector;

        // 当前选择区域
        public Rectangle CurrentSelection { get; private set; }
        public Point CurrentMouseStartPoint { get; private set; }

        // 是否正在选择
        public bool IsSelecting => _selector != null && !_selector.IsDisposed;

        // 显示选择框
        public bool ShowSelector(Action<Rectangle> action = null , Action<Point> action1 = null)
        {
            if (IsSelecting)
            {
                // 如果已显示，关闭它
                CloseSelector();
                return false;
            }

            _selector = new ScreenSelectorForm();
            _selector.FormClosed += (s, e) =>
            {
                CurrentSelection = _selector.SelectedArea;
                CurrentMouseStartPoint = _selector.MouseStartPoint;
                _selector.Dispose();
                _selector = null;
                Task.Run(() =>
                {
                    Thread.Sleep(100); // 确保选择框完全关闭
                    action?.Invoke(CurrentSelection);
                    action1?.Invoke(CurrentMouseStartPoint);
                });
            };

            _selector.Show();
            return true;
        }

        // 关闭选择框
        public void CloseSelector()
        {
            _selector?.Close();
        }

        // 获取选择区域的截图
        public Bitmap CaptureSelectedArea()
        {
            if (CurrentSelection.IsEmpty ||
                CurrentSelection.Width <= 0 ||
                CurrentSelection.Height <= 0)
            {
                return null;
            }

            try
            {
                Bitmap screenshot = new Bitmap(
                    CurrentSelection.Width,
                    CurrentSelection.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(
                        CurrentSelection.Location,
                        Point.Empty,
                        CurrentSelection.Size);
                }

                return screenshot;
            }
            catch
            {
                return null;
            }
        }

        // 根据指定区域截图
        public Bitmap CaptureArea(Rectangle area)
        {
            if (area.IsEmpty || area.Width <= 0 || area.Height <= 0)
                return null;

            Bitmap screenshot = new Bitmap(area.Width, area.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(area.Location, Point.Empty, area.Size);
            }
            return screenshot;
        }

        public void Dispose()
        {
            CloseSelector();
        }
    }
}
