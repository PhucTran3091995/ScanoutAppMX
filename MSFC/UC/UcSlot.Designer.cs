namespace MSFC.UC
{
    partial class UcSlot
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tbMain = new TableLayoutPanel();
            lbPID = new Label();
            lbWO = new Label();
            lbProgress = new Label();
            lbEBR = new Label();
            tbMain.SuspendLayout();
            SuspendLayout();
            // 
            // tbMain
            // 
            tbMain.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tbMain.ColumnCount = 1;
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbMain.Controls.Add(lbEBR, 0, 2);
            tbMain.Controls.Add(lbPID, 0, 4);
            tbMain.Controls.Add(lbWO, 0, 1);
            tbMain.Controls.Add(lbProgress, 0, 0);
            tbMain.Dock = DockStyle.Fill;
            tbMain.Location = new Point(0, 0);
            tbMain.Name = "tbMain";
            tbMain.RowCount = 5;
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tbMain.Size = new Size(155, 104);
            tbMain.TabIndex = 0;
            // 
            // lbPID
            // 
            lbPID.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbPID.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lbPID.Location = new Point(4, 81);
            lbPID.Name = "lbPID";
            lbPID.Size = new Size(147, 22);
            lbPID.TabIndex = 4;
            lbPID.Text = "XXXXXX";
            lbPID.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbWO
            // 
            lbWO.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbWO.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbWO.Location = new Point(4, 21);
            lbWO.Name = "lbWO";
            lbWO.Size = new Size(147, 19);
            lbWO.TabIndex = 1;
            lbWO.Text = "XXXXXX-XXXX";
            lbWO.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbProgress
            // 
            lbProgress.Dock = DockStyle.Fill;
            lbProgress.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbProgress.Location = new Point(4, 1);
            lbProgress.Name = "lbProgress";
            lbProgress.Size = new Size(147, 19);
            lbProgress.TabIndex = 0;
            lbProgress.Text = "xxx / xxx";
            lbProgress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbEBR
            // 
            lbEBR.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbEBR.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbEBR.Location = new Point(4, 41);
            lbEBR.Name = "lbEBR";
            lbEBR.Size = new Size(147, 19);
            lbEBR.TabIndex = 5;
            lbEBR.Text = "XXXXXX-XXXX";
            lbEBR.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // UcSlot
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            Controls.Add(tbMain);
            Name = "UcSlot";
            Size = new Size(155, 104);
            tbMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tbMain;
        public Label lbWO;
        public Label lbProgress;
        public Label lbPID;
        private Label label2;
        public Label lbEBR;
    }
}
