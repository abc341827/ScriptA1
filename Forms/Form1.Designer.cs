namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            comboBox1 = new ComboBox();
            comboBox2 = new ComboBox();
            comboBox3 = new ComboBox();
            button2 = new Button();
            listBox1 = new ListBox();
            button3 = new Button();
            label1 = new Label();
            label2 = new Label();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            comboBox4 = new ComboBox();
            comboBox5 = new ComboBox();
            comboBox6 = new ComboBox();
            button7 = new Button();
            textBox2 = new TextBox();
            label3 = new Label();
            textBox1 = new TextBox();
            label4 = new Label();
            pictureBox2 = new PictureBox();
            pictureBox3 = new PictureBox();
            button10 = new Button();
            button8 = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Enabled = false;
            button1.Location = new Point(135, 12);
            button1.Name = "button1";
            button1.Size = new Size(61, 44);
            button1.TabIndex = 0;
            button1.Text = "自动洗魔方";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "", "STR", "LUK", "INT", "DEX", "全屬性", "MaxHP", "魔法攻擊力", "物理攻擊力", "攻擊Boss怪物時傷害", "以角色等級爲準每9級 STR", "以角色等級爲準每9級 LUK", "以角色等級爲準每9級 INT", "以角色等級爲準每9級 DEX", "暴擊傷害", "技能冷卻時間" });
            comboBox1.Location = new Point(135, 74);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 25);
            comboBox1.TabIndex = 1;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "", "STR", "LUK", "INT", "DEX", "全屬性", "MaxHP", "魔法攻擊力", "物理攻擊力", "攻擊Boss怪物時傷害", "以角色等級爲準每9級 STR", "以角色等級爲準每9級 LUK", "以角色等級爲準每9級 INT", "以角色等級爲準每9級 DEX", "暴擊傷害", "技能冷卻時間" });
            comboBox2.Location = new Point(135, 105);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(121, 25);
            comboBox2.TabIndex = 2;
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Items.AddRange(new object[] { "", "STR", "LUK", "INT", "DEX", "全屬性", "MaxHP", "魔法攻擊力", "物理攻擊力", "攻擊Boss怪物時傷害", "以角色等級爲準每9級 STR", "以角色等級爲準每9級 LUK", "以角色等級爲準每9級 INT", "以角色等級爲準每9級 DEX", "暴擊傷害", "技能冷卻時間" });
            comboBox3.Location = new Point(135, 136);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(121, 25);
            comboBox3.TabIndex = 3;
            // 
            // button2
            // 
            button2.Location = new Point(135, 167);
            button2.Name = "button2";
            button2.Size = new Size(89, 23);
            button2.TabIndex = 5;
            button2.Text = "保存目標屬性";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 17;
            listBox1.Location = new Point(135, 225);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(253, 174);
            listBox1.TabIndex = 6;
            // 
            // button3
            // 
            button3.Location = new Point(136, 196);
            button3.Name = "button3";
            button3.Size = new Size(88, 23);
            button3.TabIndex = 7;
            button3.Text = "删除目标属性";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(404, 59);
            label1.Name = "label1";
            label1.Size = new Size(80, 17);
            label1.TabIndex = 8;
            label1.Text = "識別到的属性";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(404, 82);
            label2.Name = "label2";
            label2.Size = new Size(43, 17);
            label2.TabIndex = 9;
            label2.Text = "label2";
            // 
            // button4
            // 
            button4.Location = new Point(378, 12);
            button4.Name = "button4";
            button4.Size = new Size(72, 44);
            button4.TabIndex = 10;
            button4.Text = "停止洗魔方(F5)";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(15, 12);
            button5.Name = "button5";
            button5.Size = new Size(81, 45);
            button5.TabIndex = 11;
            button5.Text = "显示属性框";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Enabled = false;
            button6.Location = new Point(261, 12);
            button6.Name = "button6";
            button6.Size = new Size(61, 44);
            button6.TabIndex = 12;
            button6.Text = "自动洗怪怪";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Items.AddRange(new object[] { " ", "最終傷害", "無視怪物防禦率", "DEX", "INT", "STR", "LUK", "物理攻擊力", "魔法攻擊力", "被動技能" });
            comboBox4.Location = new Point(267, 74);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(121, 25);
            comboBox4.TabIndex = 13;
            comboBox4.Text = "最終傷害";
            // 
            // comboBox5
            // 
            comboBox5.FormattingEnabled = true;
            comboBox5.Items.AddRange(new object[] { " ", "最終傷害", "無視怪物防禦率", "DEX", "INT", "STR", "LUK", "物理攻擊力", "魔法攻擊力", "被動技能" });
            comboBox5.Location = new Point(267, 105);
            comboBox5.Name = "comboBox5";
            comboBox5.Size = new Size(121, 25);
            comboBox5.TabIndex = 14;
            comboBox5.Text = "最終傷害";
            // 
            // comboBox6
            // 
            comboBox6.FormattingEnabled = true;
            comboBox6.Items.AddRange(new object[] { " ", "最終傷害", "無視怪物防禦率", "DEX", "INT", "STR", "LUK", "物理攻擊力", "魔法攻擊力", "持續時間", "被動技能" });
            comboBox6.Location = new Point(267, 136);
            comboBox6.Name = "comboBox6";
            comboBox6.Size = new Size(121, 25);
            comboBox6.TabIndex = 15;
            // 
            // button7
            // 
            button7.Location = new Point(267, 166);
            button7.Name = "button7";
            button7.Size = new Size(89, 23);
            button7.TabIndex = 16;
            button7.Text = "保存怪怪屬性";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 415);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(100, 23);
            textBox2.TabIndex = 20;
            textBox2.Text = "MapleStory";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(14, 157);
            label3.Name = "label3";
            label3.Size = new Size(121, 17);
            label3.TabIndex = 21;
            label3.Text = "洗屬性后延遲（ms）";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(15, 177);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 22;
            textBox1.KeyPress += textBox1_KeyPress;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(404, 234);
            label4.Name = "label4";
            label4.Size = new Size(43, 17);
            label4.TabIndex = 23;
            label4.Text = "label4";
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(404, 102);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(374, 117);
            pictureBox2.TabIndex = 24;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Location = new Point(404, 254);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(374, 117);
            pictureBox3.TabIndex = 25;
            pictureBox3.TabStop = false;
            // 
            // button10
            // 
            button10.Location = new Point(15, 273);
            button10.Name = "button10";
            button10.Size = new Size(100, 47);
            button10.TabIndex = 27;
            button10.Text = "按A";
            button10.UseVisualStyleBackColor = true;
            button10.Click += button10_Click;
            // 
            // button8
            // 
            button8.Location = new Point(650, 21);
            button8.Name = "button8";
            button8.Size = new Size(75, 23);
            button8.TabIndex = 28;
            button8.Text = "ceshi ";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button8_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button8);
            Controls.Add(button10);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox2);
            Controls.Add(label4);
            Controls.Add(textBox1);
            Controls.Add(label3);
            Controls.Add(textBox2);
            Controls.Add(button7);
            Controls.Add(comboBox6);
            Controls.Add(comboBox5);
            Controls.Add(comboBox4);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button3);
            Controls.Add(listBox1);
            Controls.Add(button2);
            Controls.Add(comboBox3);
            Controls.Add(comboBox2);
            Controls.Add(comboBox1);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private ComboBox comboBox3;
        private Button button2;
        private ListBox listBox1;
        private Button button3;
        private Label label1;
        private Label label2;
        private Button button4;
        private Button button5;
        private Button button6;
        private ComboBox comboBox4;
        private ComboBox comboBox5;
        private ComboBox comboBox6;
        private Button button7;
        private TextBox textBox2;
        private Label label3;
        private TextBox textBox1;
        private Label label4;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private Button button10;
        private Button button8;
    }
}
