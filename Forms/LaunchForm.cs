using OLAPlug;
using System;
using System.Drawing;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace WinFormsApp1
{
    public class LaunchForm : Form
    {
        private Button btnForm1;
        private Button btnForm2;
        private Label label1;

        public LaunchForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            btnForm1 = new Button();
            btnForm2 = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // btnForm1
            // 
            btnForm1.Location = new Point(20, 151);
            btnForm1.Name = "btnForm1";
            btnForm1.Size = new Size(120, 36);
            btnForm1.TabIndex = 1;
            btnForm1.Text = "洗魔方";
            btnForm1.UseVisualStyleBackColor = true;
            btnForm1.Click += BtnForm1_Click;
            // 
            // btnForm2
            // 
            btnForm2.Location = new Point(161, 151);
            btnForm2.Name = "btnForm2";
            btnForm2.Size = new Size(120, 36);
            btnForm2.TabIndex = 2;
            btnForm2.Text = "黎明拍卖";
            btnForm2.UseVisualStyleBackColor = true;
            btnForm2.Click += BtnForm2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(20, 20);
            label1.Name = "label1";
            label1.Size = new Size(377, 21);
            label1.TabIndex = 0;
            label1.Text = "请选择要启动的界面 / Please choose a form to start";
            // 
            // LaunchForm
            // 
            ClientSize = new Size(484, 262);
            Controls.Add(label1);
            Controls.Add(btnForm1);
            Controls.Add(btnForm2);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LaunchForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "启动选择 / Launcher";
            ResumeLayout(false);
            PerformLayout();
        }

        private void BtnForm1_Click(object sender, EventArgs e)
        {
            try
            {
                var f1 = new Form1();
                // 当被启动窗体关闭后也关闭启动窗体
                f1.FormClosed += (s, args) => this.Close();
                this.Hide();
                f1.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法启动 Form1: {ex.Message}", "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnForm2_Click(object sender, EventArgs e)
        {
            try
            {
                var f2 = new Form2();
                f2.FormClosed += (s, args) => this.Close();
                this.Hide();
                f2.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法启动 Form2: {ex.Message}", "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
