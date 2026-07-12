namespace WinFormsApp1
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            button4 = new Button();
            richTextBox1 = new RichTextBox();
            button5 = new Button();
            button6 = new Button();
            textBox1 = new TextBox();
            button7 = new Button();
            label1 = new Label();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            label2 = new Label();
            button3 = new Button();
            detectedItemsListView = new ListView();
            purchaseTargetsListView = new ListView();
            label3 = new Label();
            textBox4 = new TextBox();
            label4 = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(131, 18);
            button1.Name = "button1";
            button1.Size = new Size(112, 45);
            button1.TabIndex = 0;
            button1.Text = "開始執行";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(13, 18);
            button2.Name = "button2";
            button2.Size = new Size(112, 45);
            button2.TabIndex = 2;
            button2.Text = "寻找窗口";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button4
            // 
            button4.Location = new Point(131, 69);
            button4.Name = "button4";
            button4.Size = new Size(113, 45);
            button4.TabIndex = 5;
            button4.Text = "停止";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(13, 250);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(330, 174);
            richTextBox1.TabIndex = 6;
            richTextBox1.Text = "";
            // 
            // button5
            // 
            button5.Location = new Point(13, 69);
            button5.Name = "button5";
            button5.Size = new Size(112, 45);
            button5.TabIndex = 22;
            button5.Text = "取边框色值";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Location = new Point(346, 18);
            button6.Name = "button6";
            button6.Size = new Size(75, 23);
            button6.TabIndex = 23;
            button6.Text = "添加";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(346, 75);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(114, 23);
            textBox1.TabIndex = 24;
            textBox1.Text = "配件指南";
            // 
            // button7
            // 
            button7.Location = new Point(346, 47);
            button7.Name = "button7";
            button7.Size = new Size(75, 23);
            button7.TabIndex = 25;
            button7.Text = "刪除";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(21, 123);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 26;
            label1.Text = "下滑次数";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(83, 120);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(100, 23);
            textBox2.TabIndex = 27;
            textBox2.Text = "5";
            textBox2.KeyPress += textBox2_KeyPress;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(251, 120);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(100, 23);
            textBox3.TabIndex = 28;
            textBox3.Text = "6";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(189, 123);
            label2.Name = "label2";
            label2.Size = new Size(56, 17);
            label2.TabIndex = 29;
            label2.Text = "下滑幅度";
            // 
            // button3
            // 
            button3.Location = new Point(716, 20);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 30;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // detectedItemsListView
            // 
            detectedItemsListView.FullRowSelect = true;
            detectedItemsListView.GridLines = true;
            detectedItemsListView.Location = new Point(360, 190);
            detectedItemsListView.Name = "detectedItemsListView";
            detectedItemsListView.Size = new Size(416, 234);
            detectedItemsListView.TabIndex = 32;
            detectedItemsListView.UseCompatibleStateImageBehavior = false;
            detectedItemsListView.View = View.Details;
            detectedItemsListView.Columns.Add("名称", 140);
            detectedItemsListView.Columns.Add("价格", 80);
            detectedItemsListView.Columns.Add("最低价数量", 95);
            detectedItemsListView.Columns.Add("剩余数量", 95);
            // 
            // purchaseTargetsListView
            // 
            purchaseTargetsListView.FullRowSelect = true;
            purchaseTargetsListView.GridLines = true;
            purchaseTargetsListView.Location = new Point(466, 18);
            purchaseTargetsListView.Name = "purchaseTargetsListView";
            purchaseTargetsListView.Size = new Size(310, 140);
            purchaseTargetsListView.TabIndex = 33;
            purchaseTargetsListView.UseCompatibleStateImageBehavior = false;
            purchaseTargetsListView.View = View.Details;
            purchaseTargetsListView.Columns.Add("名称", 130);
            purchaseTargetsListView.Columns.Add("最高价格", 90);
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(296, 78);
            label3.Name = "label3";
            label3.Size = new Size(44, 17);
            label3.TabIndex = 34;
            label3.Text = "道具名";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(346, 104);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(114, 23);
            textBox4.TabIndex = 35;
            textBox4.Text = "40000";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(296, 107);
            label4.Name = "label4";
            label4.Size = new Size(44, 17);
            label4.TabIndex = 36;
            label4.Text = "最高价";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label4);
            Controls.Add(textBox4);
            Controls.Add(label3);
            Controls.Add(purchaseTargetsListView);
            Controls.Add(detectedItemsListView);
            Controls.Add(button3);
            Controls.Add(label2);
            Controls.Add(textBox3);
            Controls.Add(textBox2);
            Controls.Add(label1);
            Controls.Add(button7);
            Controls.Add(textBox1);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(richTextBox1);
            Controls.Add(button4);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form2";
            Text = "Form2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button4;
        private RichTextBox richTextBox1;
        private Button button5;
        private Button button6;
        private TextBox textBox1;
        private Button button7;
        private Label label1;
        private TextBox textBox2;
        private TextBox textBox3;
        private Label label2;
        private Button button3;
        private ListView detectedItemsListView;
        private ListView purchaseTargetsListView;
        private Label label3;
        private TextBox textBox4;
        private Label label4;
    }
}