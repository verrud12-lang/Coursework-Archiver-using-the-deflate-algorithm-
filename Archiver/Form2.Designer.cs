namespace Archiver
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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            label7 = new Label();
            label8 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(47, 126);
            label1.Name = "label1";
            label1.Size = new Size(226, 40);
            label1.TabIndex = 0;
            label1.Text = "Выбран файл:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(311, 126);
            label2.Name = "label2";
            label2.Size = new Size(181, 40);
            label2.TabIndex = 1;
            label2.Text = "Имя файла";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(53, 191);
            label3.Name = "label3";
            label3.Size = new Size(96, 40);
            label3.TabIndex = 2;
            label3.Text = "Путь:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(311, 191);
            label4.Name = "label4";
            label4.Size = new Size(214, 40);
            label4.TabIndex = 3;
            label4.Text = "Путь к файлу";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(55, 263);
            label5.Name = "label5";
            label5.Size = new Size(81, 40);
            label5.TabIndex = 4;
            label5.Text = "Вес:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(311, 263);
            label6.Name = "label6";
            label6.Size = new Size(249, 40);
            label6.TabIndex = 5;
            label6.Text = "Вес файла в мб";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Font = new Font("Trebuchet MS", 12F);
            button1.ImageAlign = ContentAlignment.TopRight;
            button1.Location = new Point(1175, 126);
            button1.Name = "button1";
            button1.Size = new Size(440, 46);
            button1.TabIndex = 6;
            button1.Text = "Распаковать";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.Font = new Font("Trebuchet MS", 12F);
            button2.ImageAlign = ContentAlignment.TopRight;
            button2.Location = new Point(1175, 221);
            button2.Name = "button2";
            button2.Size = new Size(440, 46);
            button2.TabIndex = 7;
            button2.Text = "Заархивировать с сжатием";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button3.Font = new Font("Trebuchet MS", 12F);
            button3.ImageAlign = ContentAlignment.TopRight;
            button3.Location = new Point(1175, 311);
            button3.Name = "button3";
            button3.Size = new Size(440, 46);
            button3.TabIndex = 8;
            button3.Text = "Заархивировать без сжатия";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button4.Location = new Point(1175, 397);
            button4.Name = "button4";
            button4.Size = new Size(440, 46);
            button4.TabIndex = 9;
            button4.Text = "Очистить";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Trebuchet MS", 16.125F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(47, 32);
            label7.Name = "label7";
            label7.Size = new Size(362, 54);
            label7.TabIndex = 10;
            label7.Text = "Краткая сводка:";
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Font = new Font("Trebuchet MS", 16.125F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label8.ImageAlign = ContentAlignment.TopRight;
            label8.Location = new Point(1175, 32);
            label8.Name = "label8";
            label8.Size = new Size(475, 54);
            label8.TabIndex = 11;
            label8.Text = "Доступные действия:";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(17F, 40F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.AntiqueWhite;
            ClientSize = new Size(1731, 808);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Trebuchet MS", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ForeColor = Color.DarkBlue;
            Margin = new Padding(4);
            Name = "Form2";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Выбор действия";
            Load += Form2_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Label label7;
        private Label label8;
    }
}