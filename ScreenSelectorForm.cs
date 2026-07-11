using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinFormsApp1
{
    // ScreenSelectorForm.cs
    public partial class ScreenSelectorForm : Form
    {
        // 选择区域
        private Rectangle _selectionRect;

        // 鼠标状态
        private Point _mouseStartPoint;
        private bool _isDragging = false;
        private bool _isResizing = false;
        private ResizeDirection _resizeDirection = ResizeDirection.None;

        // 样式
        private Pen _borderPen;
        private Brush _fillBrush;
        private Color _borderColor = Color.Red;

        // 手柄大小
        private const int GripSize = 8;

        // 调整方向枚举
        private enum ResizeDirection
        {
            None,
            Top,
            Bottom,
            Left,
            Right,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        // 属性：获取选择区域（屏幕坐标）
        public Rectangle SelectedArea => _selectionRect;
        public Point MouseStartPoint => _mouseStartPoint;
        public ScreenSelectorForm()
        {
            InitializeForm();
            InitializeDrawing();
        }

        private void InitializeForm()
        {
            // 窗体设置
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Cross;

            // 覆盖全屏
            var screenBounds = Screen.PrimaryScreen.Bounds;
            this.Bounds = screenBounds;

            // 透明背景（50%黑）
            this.BackColor = Color.Black;
            this.Opacity = 0.3;
            this.TransparencyKey = Color.Empty;

            // 鼠标事件
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
            this.Paint += OnPaint;

            // 键盘支持
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };
        }

        private void InitializeDrawing()
        {
            _borderPen = new Pen(_borderColor, 2);
            _fillBrush = new SolidBrush(Color.FromArgb(30, Color.White));
        }

        private ResizeDirection GetResizeDirection(Point mousePoint)
        {
            if (_selectionRect.IsEmpty)
                return ResizeDirection.None;

            // 检查四个角
            Rectangle topLeft = new Rectangle(
                _selectionRect.Left - GripSize / 2,
                _selectionRect.Top - GripSize / 2,
                GripSize, GripSize);

            Rectangle topRight = new Rectangle(
                _selectionRect.Right - GripSize / 2,
                _selectionRect.Top - GripSize / 2,
                GripSize, GripSize);

            Rectangle bottomLeft = new Rectangle(
                _selectionRect.Left - GripSize / 2,
                _selectionRect.Bottom - GripSize / 2,
                GripSize, GripSize);

            Rectangle bottomRight = new Rectangle(
                _selectionRect.Right - GripSize / 2,
                _selectionRect.Bottom - GripSize / 2,
                GripSize, GripSize);

            if (topLeft.Contains(mousePoint)) return ResizeDirection.TopLeft;
            if (topRight.Contains(mousePoint)) return ResizeDirection.TopRight;
            if (bottomLeft.Contains(mousePoint)) return ResizeDirection.BottomLeft;
            if (bottomRight.Contains(mousePoint)) return ResizeDirection.BottomRight;

            // 检查四条边
            Rectangle topEdge = new Rectangle(
                _selectionRect.Left + GripSize,
                _selectionRect.Top - GripSize / 2,
                _selectionRect.Width - GripSize * 2,
                GripSize);

            Rectangle bottomEdge = new Rectangle(
                _selectionRect.Left + GripSize,
                _selectionRect.Bottom - GripSize / 2,
                _selectionRect.Width - GripSize * 2,
                GripSize);

            Rectangle leftEdge = new Rectangle(
                _selectionRect.Left - GripSize / 2,
                _selectionRect.Top + GripSize,
                GripSize,
                _selectionRect.Height - GripSize * 2);

            Rectangle rightEdge = new Rectangle(
                _selectionRect.Right - GripSize / 2,
                _selectionRect.Top + GripSize,
                GripSize,
                _selectionRect.Height - GripSize * 2);

            if (topEdge.Contains(mousePoint)) return ResizeDirection.Top;
            if (bottomEdge.Contains(mousePoint)) return ResizeDirection.Bottom;
            if (leftEdge.Contains(mousePoint)) return ResizeDirection.Left;
            if (rightEdge.Contains(mousePoint)) return ResizeDirection.Right;

            // 检查是否在矩形内部（用于拖动）
            if (_selectionRect.Contains(mousePoint))
                return ResizeDirection.None;

            return ResizeDirection.None;
        }

        private void UpdateCursor(ResizeDirection direction)
        {
            switch (direction)
            {
                case ResizeDirection.Top:
                case ResizeDirection.Bottom:
                    this.Cursor = Cursors.SizeNS;
                    break;
                case ResizeDirection.Left:
                case ResizeDirection.Right:
                    this.Cursor = Cursors.SizeWE;
                    break;
                case ResizeDirection.TopLeft:
                case ResizeDirection.BottomRight:
                    this.Cursor = Cursors.SizeNWSE;
                    break;
                case ResizeDirection.TopRight:
                case ResizeDirection.BottomLeft:
                    this.Cursor = Cursors.SizeNESW;
                    break;
                case ResizeDirection.None:
                    this.Cursor = Cursors.SizeAll;
                    break;
                default:
                    this.Cursor = Cursors.Default;
                    break;
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            _mouseStartPoint = e.Location;
            _resizeDirection = GetResizeDirection(e.Location);

            if (_resizeDirection != ResizeDirection.None)
            {
                // 开始调整大小
                _isResizing = true;
                UpdateCursor(_resizeDirection);
            }
            else if (!_selectionRect.IsEmpty && _selectionRect.Contains(e.Location))
            {
                // 开始拖动
                _isDragging = true;
                this.Cursor = Cursors.SizeAll;
            }
            else
            {
                // 开始绘制新区域
                _selectionRect = new Rectangle(e.Location, Size.Empty);
                _isResizing = true;
                _resizeDirection = ResizeDirection.BottomRight; // 默认从右下角扩展
                this.Cursor = Cursors.Cross;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing && _resizeDirection != ResizeDirection.None)
            {
                // 调整大小
                int deltaX = e.X - _mouseStartPoint.X;
                int deltaY = e.Y - _mouseStartPoint.Y;

                Rectangle newRect = _selectionRect;

                switch (_resizeDirection)
                {
                    case ResizeDirection.Top:
                        newRect.Y += deltaY;
                        newRect.Height -= deltaY;
                        break;
                    case ResizeDirection.Bottom:
                        newRect.Height += deltaY;
                        break;
                    case ResizeDirection.Left:
                        newRect.X += deltaX;
                        newRect.Width -= deltaX;
                        break;
                    case ResizeDirection.Right:
                        newRect.Width += deltaX;
                        break;
                    case ResizeDirection.TopLeft:
                        newRect.X += deltaX;
                        newRect.Width -= deltaX;
                        newRect.Y += deltaY;
                        newRect.Height -= deltaY;
                        break;
                    case ResizeDirection.TopRight:
                        newRect.Width += deltaX;
                        newRect.Y += deltaY;
                        newRect.Height -= deltaY;
                        break;
                    case ResizeDirection.BottomLeft:
                        newRect.X += deltaX;
                        newRect.Width -= deltaX;
                        newRect.Height += deltaY;
                        break;
                    case ResizeDirection.BottomRight:
                        newRect.Width += deltaX;
                        newRect.Height += deltaY;
                        break;
                }

                // 确保最小尺寸
                if (newRect.Width >= 10 && newRect.Height >= 10)
                {
                    _selectionRect = newRect;      
                    _mouseStartPoint = e.Location;
                    this.Invalidate();
                }
            }
            else if (_isDragging)
            {
                // 拖动整个区域
                int deltaX = e.X - _mouseStartPoint.X;
                int deltaY = e.Y - _mouseStartPoint.Y;

                _selectionRect.X += deltaX;
                _selectionRect.Y += deltaY;
                _mouseStartPoint = e.Location;
                this.Invalidate();
            }
            else
            {
                // 更新光标
                var direction = GetResizeDirection(e.Location);
                UpdateCursor(direction);
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _isResizing = false;
            _resizeDirection = ResizeDirection.None;

            // 如果区域太小，清空
            if (_selectionRect.Width < 10 || _selectionRect.Height < 10)
            {
                _selectionRect = Rectangle.Empty;
                this.Invalidate();
            }

            this.Close();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 绘制半透明背景（清除之前的绘制）
            g.Clear(Color.FromArgb(50, 0, 0, 0));

            // 如果有选择区域，绘制它
            if (!_selectionRect.IsEmpty && _selectionRect.Width > 0 && _selectionRect.Height > 0)
            {
                // 绘制半透明填充
                g.FillRectangle(_fillBrush, _selectionRect);

                // 绘制边框
                g.DrawRectangle(_borderPen, _selectionRect);

                // 在左上角显示尺寸
                string sizeText = $"{_selectionRect.Width} × {_selectionRect.Height}";
                using (var font = new Font("Arial", 10))
                {
                    var size = g.MeasureString(sizeText, font);
                    var textPoint = new Point(_selectionRect.X, _selectionRect.Y - (int)size.Height - 5);

                    // 如果文字超出上方，放在下方
                    if (textPoint.Y < 0)
                    {
                        textPoint = new Point(_selectionRect.X, _selectionRect.Bottom + 5);
                    }

                    // 绘制文字背景
                    g.FillRectangle(Brushes.Black,
                        textPoint.X, textPoint.Y, size.Width, size.Height);

                    // 绘制文字
                    g.DrawString(sizeText, font, Brushes.White, textPoint);
                }
            }
        }
    }
}
