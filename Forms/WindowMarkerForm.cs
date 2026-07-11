using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class WindowMarkerForm : Form
    {
        private const int MarkerRadius = 10;
        private List<Point> _markers;
        private Rectangle targetRec;
        private int _dragIndex = -1;
        private Point _dragOffset;

        private Button _btnSave;
        private Button _btnCancel;
        private Label _lblHint;

        public IReadOnlyList<Point> Markers => _markers;

        public WindowMarkerForm(Rectangle targetRec, List<Point> initialMarkers)
        {
            var bounds = targetRec; // 已经是屏幕坐标
            var defaultPoints = new List<Point>
                    {
                        new Point(bounds.Left + initialMarkers[0].X, bounds.Top +initialMarkers[0].Y),//装备培养(刷新)
                        new Point(bounds.Left +initialMarkers[1].X, bounds.Top +initialMarkers[1].Y),//最大
                        new Point(bounds.Left + initialMarkers[2].X, bounds.Top +initialMarkers[2].Y)//确定
                    };

            _markers = defaultPoints?.Select(p => new Point(p.X, p.Y)).ToList() ?? new List<Point>();

            InitializeForm();
            this.targetRec = targetRec;
        }

        private void InitializeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.BackColor = Color.Black;
            this.Opacity = 0.25;
            this.Cursor = Cursors.Cross;

            this.MouseDown += WindowMarkerForm_MouseDown;
            this.MouseMove += WindowMarkerForm_MouseMove;
            this.MouseUp += WindowMarkerForm_MouseUp;
            this.MouseDoubleClick += WindowMarkerForm_MouseDoubleClick;
            this.Paint += WindowMarkerForm_Paint;

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

            // Save / Cancel buttons
            _btnSave = new Button { Text = "保存", Width = 80, Height = 30 };
            _btnCancel = new Button { Text = "取消", Width = 80, Height = 30 };
            _lblHint = new Label { AutoSize = false, Width = 400, Height = 30 };

            _lblHint.Text = "双击添加标记，右键删除，拖动移动。按 Enter 保存，Esc 取消。";
            _lblHint.ForeColor = Color.White;

            _btnSave.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
            _btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnCancel);
            this.Controls.Add(_lblHint);

            _btnSave.Location = new Point(this.ClientSize.Width - _btnSave.Width - 20, 20);
            _btnCancel.Location = new Point(this.ClientSize.Width - _btnCancel.Width - _btnSave.Width - 30, 20);
            _lblHint.Location = new Point(20, 20);

            // Ensure controls are drawn on top
            _btnSave.BringToFront();
            _btnCancel.BringToFront();
            _lblHint.BringToFront();

            this.Resize += (s, e) =>
            {
                _btnSave.Location = new Point(this.ClientSize.Width - _btnSave.Width - 20, 20);
                _btnCancel.Location = new Point(this.ClientSize.Width - _btnCancel.Width - _btnSave.Width - 30, 20);
            };
        }

        private void WindowMarkerForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 添加新标记
                var p = new Point(e.X, e.Y);
                _markers.Add(p);
                this.Invalidate();
            }
        }

        private void WindowMarkerForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 查找是否点击到现有标记
                for (int i = 0; i < _markers.Count; i++)
                {
                    var m = _markers[i];
                    var dx = e.X - m.X;
                    var dy = e.Y - m.Y;
                    if (dx * dx + dy * dy <= MarkerRadius * MarkerRadius)
                    {
                        _dragIndex = i;
                        _dragOffset = new Point(dx, dy);
                        this.Cursor = Cursors.SizeAll;
                        return;
                    }
                }

                // 如果没有点击到标记，允许通过左键拖动选择（不实现）
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 右键删除命中标记
                for (int i = 0; i < _markers.Count; i++)
                {
                    var m = _markers[i];
                    var dx = e.X - m.X;
                    var dy = e.Y - m.Y;
                    if (dx * dx + dy * dy <= MarkerRadius * MarkerRadius)
                    {
                        _markers.RemoveAt(i);
                        this.Invalidate();
                        return;
                    }
                }
            }
        }

        private void WindowMarkerForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragIndex >= 0)
            {
                var newPos = new Point(e.X - _dragOffset.X, e.Y - _dragOffset.Y);
                // 限制在屏幕内
                newPos.X = Math.Max(0, Math.Min(this.ClientSize.Width, newPos.X));
                newPos.Y = Math.Max(0, Math.Min(this.ClientSize.Height, newPos.Y));
                _markers[_dragIndex] = newPos;
                this.Invalidate();
            }
            else
            {
                // 更新光标样式，如果悬停在标记上
                bool over = _markers.Any(m => (m.X - e.X) * (m.X - e.X) + (m.Y - e.Y) * (m.Y - e.Y) <= MarkerRadius * MarkerRadius);
                this.Cursor = over ? Cursors.Hand : Cursors.Cross;
            }
        }

        private void WindowMarkerForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (_dragIndex >= 0)
            {
                _dragIndex = -1;
                this.Cursor = Cursors.Cross;
            }
        }

        private void WindowMarkerForm_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            // 绘制半透明遮罩（已经由 Opacity 实现，但可再绘制一些指示）
            // 绘制标记
            for (int i = 0; i < _markers.Count; i++)
            {
                var m = _markers[i];
                var rect = new Rectangle(m.X - MarkerRadius, m.Y - MarkerRadius, MarkerRadius * 2, MarkerRadius * 2);
                using (var brush = new SolidBrush(Color.FromArgb(200, 255, 120, 0)))
                using (var pen = new Pen(Color.Yellow, 2))
                {
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(pen, rect);
                }

                // 绘制编号
                var label = (i + 1).ToString();
                using (var font = new Font("Arial", 10, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var brush = new SolidBrush(Color.Black))
                {
                    g.DrawString(label, font, brush, m.X, m.Y, sf);
                }
            }
        }
    }
}
