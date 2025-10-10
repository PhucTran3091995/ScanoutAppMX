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
            tbControl = new TableLayoutPanel();
            btnStart = new Button();
            rtxtLog = new RichTextBox();
            tbMain.SuspendLayout();
            tbControl.SuspendLayout();
            SuspendLayout();
            // 
            // tbMain
            // 
            tbMain.ColumnCount = 2;
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbMain.Controls.Add(tbControl, 0, 0);
            tbMain.Controls.Add(rtxtLog, 1, 0);
            tbMain.Dock = DockStyle.Fill;
            tbMain.Location = new Point(0, 0);
            tbMain.Name = "tbMain";
            tbMain.RowCount = 1;
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbMain.Size = new Size(800, 450);
            tbMain.TabIndex = 0;
            // 
            // tbControl
            // 
            tbControl.ColumnCount = 1;
            tbControl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbControl.Controls.Add(btnStart, 0, 0);
            tbControl.Dock = DockStyle.Fill;
            tbControl.Location = new Point(3, 3);
            tbControl.Name = "tbControl";
            tbControl.RowCount = 2;
            tbControl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tbControl.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbControl.Size = new Size(94, 444);
            tbControl.TabIndex = 0;
            // 
            // btnStart
            // 
            btnStart.Dock = DockStyle.Fill;
            btnStart.Location = new Point(3, 3);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(88, 44);
            btnStart.TabIndex = 0;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // rtxtLog
            // 
            rtxtLog.BorderStyle = BorderStyle.FixedSingle;
            rtxtLog.Dock = DockStyle.Fill;
            rtxtLog.Location = new Point(103, 3);
            rtxtLog.Name = "rtxtLog";
            rtxtLog.Size = new Size(694, 444);
            rtxtLog.TabIndex = 1;
            rtxtLog.Text = "";
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
            Resize += frmKLAS_Resize;
            tbMain.ResumeLayout(false);
            tbControl.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tbMain;
        private TableLayoutPanel tbControl;
        private Button btnStart;
        private RichTextBox rtxtLog;
    }
}