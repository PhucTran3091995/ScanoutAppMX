namespace MSFC
{
    partial class frmNotice
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
            btnConfirm = new Button();
            label1 = new Label();
            lbNoticeMsg = new Label();
            lblEBR = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnConfirm
            // 
            btnConfirm.BackColor = Color.Chartreuse;
            btnConfirm.Dock = DockStyle.Fill;
            btnConfirm.Font = new Font("Segoe UI", 36F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnConfirm.Location = new Point(3, 405);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(1190, 130);
            btnConfirm.TabIndex = 2;
            btnConfirm.Text = "Clasificado";
            btnConfirm.UseVisualStyleBackColor = false;
            btnConfirm.Click += btnConfirm_Click;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Arial", 36F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Red;
            label1.Location = new Point(3, 134);
            label1.Name = "label1";
            label1.Size = new Size(1190, 134);
            label1.TabIndex = 1;
            label1.Text = "⚠️ Clasifíquelo para evitar confusiones.";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbNoticeMsg
            // 
            lbNoticeMsg.Dock = DockStyle.Fill;
            lbNoticeMsg.Font = new Font("Arial", 36F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lbNoticeMsg.ForeColor = Color.Red;
            lbNoticeMsg.Location = new Point(3, 0);
            lbNoticeMsg.Name = "lbNoticeMsg";
            lbNoticeMsg.Size = new Size(1190, 134);
            lbNoticeMsg.TabIndex = 0;
            lbNoticeMsg.Text = "⚠️ Ha escaneado un EBR distinto al registrado. ";
            lbNoticeMsg.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblEBR
            // 
            lblEBR.Dock = DockStyle.Fill;
            lblEBR.Font = new Font("Arial", 36F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblEBR.ForeColor = Color.Red;
            lblEBR.Location = new Point(3, 268);
            lblEBR.Name = "lblEBR";
            lblEBR.Size = new Size(1190, 134);
            lblEBR.TabIndex = 3;
            lblEBR.Text = "......................";
            lblEBR.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.FromArgb(255, 255, 128);
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(btnConfirm, 0, 3);
            tableLayoutPanel1.Controls.Add(lblEBR, 0, 2);
            tableLayoutPanel1.Controls.Add(lbNoticeMsg, 0, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Size = new Size(1196, 538);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // frmNotice
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1196, 538);
            Controls.Add(tableLayoutPanel1);
            Name = "frmNotice";
            Text = "frmNotice";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        public Label lbNoticeMsg;
        public Label label1;
        private Button btnConfirm;
        public Label lblEBR;
        private TableLayoutPanel tableLayoutPanel1;
    }
}