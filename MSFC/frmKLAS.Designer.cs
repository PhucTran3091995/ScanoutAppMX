namespace MSFC
{
    partial class frmKLAS
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
            tbMain = new TableLayoutPanel();
            btnStart = new Button();
            rtxtLog = new RichTextBox();
            label1 = new Label();
            numInterval = new NumericUpDown();
            tbMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).BeginInit();
            SuspendLayout();
            // 
            // tbMain
            // 
            tbMain.ColumnCount = 2;
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbMain.Controls.Add(btnStart, 0, 1);
            tbMain.Controls.Add(rtxtLog, 1, 1);
            tbMain.Controls.Add(label1, 0, 0);
            tbMain.Controls.Add(numInterval, 1, 0);
            tbMain.Dock = DockStyle.Fill;
            tbMain.Location = new Point(0, 0);
            tbMain.Name = "tbMain";
            tbMain.RowCount = 3;
            tbMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tbMain.Size = new Size(800, 450);
            tbMain.TabIndex = 0;
            // 
            // btnStart
            // 
            btnStart.Dock = DockStyle.Fill;
            btnStart.Location = new Point(3, 33);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(94, 394);
            btnStart.TabIndex = 0;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // rtxtLog
            // 
            rtxtLog.BorderStyle = BorderStyle.FixedSingle;
            rtxtLog.Dock = DockStyle.Fill;
            rtxtLog.Location = new Point(103, 33);
            rtxtLog.Name = "rtxtLog";
            rtxtLog.Size = new Size(694, 394);
            rtxtLog.TabIndex = 1;
            rtxtLog.Text = "";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(94, 30);
            label1.TabIndex = 2;
            label1.Text = "Interval (sec)";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // numInterval
            // 
            numInterval.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            numInterval.Location = new Point(103, 3);
            numInterval.Maximum = new decimal(new int[] { 186400, 0, 0, 0 });
            numInterval.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numInterval.Name = "numInterval";
            numInterval.Size = new Size(167, 23);
            numInterval.TabIndex = 3;
            numInterval.Value = new decimal(new int[] { 7200, 0, 0, 0 });
            numInterval.ValueChanged += numInterval_ValueChanged;
            // 
            // frmKLAS
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tbMain);
            Name = "frmKLAS";
            Text = "frmKLAS";
            FormClosing += frmKLAS_FormClosing;
            Load += frmKLAS_Load;
            Resize += frmKLAS_Resize;
            tbMain.ResumeLayout(false);
            tbMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tbMain;
        private Button btnStart;
        private RichTextBox rtxtLog;
        private Label label1;
        private NumericUpDown numInterval;
    }
}