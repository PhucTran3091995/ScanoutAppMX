namespace MSFC
{
    partial class frmTraceAndPrint
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
            lbStatus = new Label();
            btnPrint = new Button();
            tbMain = new TableLayoutPanel();
            tbContent = new TableLayoutPanel();
            tbMain_Row3 = new TableLayoutPanel();
            tbData = new TableLayoutPanel();
            tbRight = new TableLayoutPanel();
            pnStatus = new Panel();
            rtxtDetailExplain = new RichTextBox();
            txtScanPid = new TextBox();
            panel2 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnClear = new Button();
            label1 = new Label();
            lstScanedPids = new ListBox();
            panel1 = new Panel();
            lbModelInfor = new Label();
            tbMain.SuspendLayout();
            tbContent.SuspendLayout();
            tbMain_Row3.SuspendLayout();
            tbData.SuspendLayout();
            tbRight.SuspendLayout();
            pnStatus.SuspendLayout();
            panel2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lbStatus
            // 
            lbStatus.BackColor = Color.Transparent;
            lbStatus.Dock = DockStyle.Fill;
            lbStatus.Font = new Font("Bookman Old Style", 48F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbStatus.ForeColor = Color.Yellow;
            lbStatus.Location = new Point(0, 0);
            lbStatus.Margin = new Padding(4, 0, 4, 0);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new Size(1333, 157);
            lbStatus.TabIndex = 1;
            lbStatus.Text = "LISTA";
            lbStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnPrint
            // 
            btnPrint.BackColor = Color.DodgerBlue;
            btnPrint.Dock = DockStyle.Fill;
            btnPrint.FlatAppearance.MouseDownBackColor = Color.Green;
            btnPrint.FlatAppearance.MouseOverBackColor = Color.SteelBlue;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Arial", 27.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnPrint.ForeColor = SystemColors.ButtonHighlight;
            btnPrint.Location = new Point(4, 765);
            btnPrint.Margin = new Padding(4, 5, 4, 5);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(1333, 157);
            btnPrint.TabIndex = 2;
            btnPrint.Text = "🖨️ Imprimir Etiqueta Ahora";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // tbMain
            // 
            tbMain.ColumnCount = 1;
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbMain.Controls.Add(tbContent, 0, 1);
            tbMain.Controls.Add(panel1, 0, 0);
            tbMain.Dock = DockStyle.Fill;
            tbMain.Location = new Point(0, 0);
            tbMain.Margin = new Padding(4, 5, 4, 5);
            tbMain.Name = "tbMain";
            tbMain.RowCount = 2;
            tbMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 83F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tbMain.Size = new Size(1813, 1050);
            tbMain.TabIndex = 2;
            // 
            // tbContent
            // 
            tbContent.ColumnCount = 1;
            tbContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbContent.Controls.Add(tbMain_Row3, 0, 0);
            tbContent.Dock = DockStyle.Fill;
            tbContent.Location = new Point(4, 88);
            tbContent.Margin = new Padding(4, 5, 4, 5);
            tbContent.Name = "tbContent";
            tbContent.RowCount = 1;
            tbContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbContent.Size = new Size(1805, 957);
            tbContent.TabIndex = 3;
            // 
            // tbMain_Row3
            // 
            tbMain_Row3.ColumnCount = 1;
            tbMain_Row3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbMain_Row3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 29F));
            tbMain_Row3.Controls.Add(tbData, 0, 0);
            tbMain_Row3.Dock = DockStyle.Fill;
            tbMain_Row3.Location = new Point(4, 5);
            tbMain_Row3.Margin = new Padding(4, 5, 4, 5);
            tbMain_Row3.Name = "tbMain_Row3";
            tbMain_Row3.RowCount = 1;
            tbMain_Row3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbMain_Row3.RowStyles.Add(new RowStyle(SizeType.Absolute, 947F));
            tbMain_Row3.Size = new Size(1797, 947);
            tbMain_Row3.TabIndex = 0;
            // 
            // tbData
            // 
            tbData.ColumnCount = 2;
            tbData.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440F));
            tbData.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbData.Controls.Add(tbRight, 1, 0);
            tbData.Controls.Add(panel2, 0, 0);
            tbData.Dock = DockStyle.Fill;
            tbData.Location = new Point(4, 5);
            tbData.Margin = new Padding(4, 5, 4, 5);
            tbData.Name = "tbData";
            tbData.RowCount = 1;
            tbData.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbData.Size = new Size(1789, 937);
            tbData.TabIndex = 1;
            // 
            // tbRight
            // 
            tbRight.ColumnCount = 1;
            tbRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbRight.Controls.Add(pnStatus, 0, 0);
            tbRight.Controls.Add(rtxtDetailExplain, 0, 2);
            tbRight.Controls.Add(txtScanPid, 0, 1);
            tbRight.Controls.Add(btnPrint, 0, 3);
            tbRight.Dock = DockStyle.Fill;
            tbRight.Location = new Point(444, 5);
            tbRight.Margin = new Padding(4, 5, 4, 5);
            tbRight.Name = "tbRight";
            tbRight.RowCount = 4;
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 167F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 167F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tbRight.Size = new Size(1341, 927);
            tbRight.TabIndex = 1;
            // 
            // pnStatus
            // 
            pnStatus.BackColor = Color.Black;
            pnStatus.Controls.Add(lbStatus);
            pnStatus.Dock = DockStyle.Fill;
            pnStatus.Location = new Point(4, 5);
            pnStatus.Margin = new Padding(4, 5, 4, 5);
            pnStatus.Name = "pnStatus";
            pnStatus.Size = new Size(1333, 157);
            pnStatus.TabIndex = 0;
            // 
            // rtxtDetailExplain
            // 
            rtxtDetailExplain.Dock = DockStyle.Fill;
            rtxtDetailExplain.Enabled = false;
            rtxtDetailExplain.Location = new Point(4, 240);
            rtxtDetailExplain.Margin = new Padding(4, 5, 4, 5);
            rtxtDetailExplain.Name = "rtxtDetailExplain";
            rtxtDetailExplain.Size = new Size(1333, 515);
            rtxtDetailExplain.TabIndex = 4;
            rtxtDetailExplain.Text = "";
            // 
            // txtScanPid
            // 
            txtScanPid.Dock = DockStyle.Fill;
            txtScanPid.Font = new Font("Arial", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtScanPid.Location = new Point(4, 172);
            txtScanPid.Margin = new Padding(4, 5, 4, 5);
            txtScanPid.Name = "txtScanPid";
            txtScanPid.Size = new Size(1333, 49);
            txtScanPid.TabIndex = 5;
            txtScanPid.KeyPress += textBox1_KeyPress;
            txtScanPid.Validated += textBox1_Validated;
            // 
            // panel2
            // 
            panel2.Controls.Add(tableLayoutPanel1);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(4, 5);
            panel2.Margin = new Padding(4, 5, 4, 5);
            panel2.Name = "panel2";
            panel2.Size = new Size(432, 927);
            panel2.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(btnClear, 0, 2);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(lstScanedPids, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 5, 4, 5);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 6.4453125F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 93.55469F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 163F));
            tableLayoutPanel1.Size = new Size(432, 927);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.Azure;
            btnClear.Dock = DockStyle.Fill;
            btnClear.FlatAppearance.MouseDownBackColor = Color.Green;
            btnClear.FlatAppearance.MouseOverBackColor = Color.SteelBlue;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Font = new Font("Arial", 21.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnClear.ForeColor = SystemColors.ActiveCaptionText;
            btnClear.Location = new Point(4, 768);
            btnClear.Margin = new Padding(4, 5, 4, 5);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(424, 154);
            btnClear.TabIndex = 3;
            btnClear.Text = "Eliminar mensaje de error";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += btnClear_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Arial Narrow", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(4, 0);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(424, 49);
            label1.TabIndex = 0;
            label1.Text = "lista de espera para impresión de sellos";
            // 
            // lstScanedPids
            // 
            lstScanedPids.Dock = DockStyle.Fill;
            lstScanedPids.FormattingEnabled = true;
            lstScanedPids.ItemHeight = 25;
            lstScanedPids.Location = new Point(4, 54);
            lstScanedPids.Margin = new Padding(4, 5, 4, 5);
            lstScanedPids.Name = "lstScanedPids";
            lstScanedPids.Size = new Size(424, 704);
            lstScanedPids.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(255, 255, 128);
            panel1.Controls.Add(lbModelInfor);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(4, 5);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(1805, 73);
            panel1.TabIndex = 4;
            // 
            // lbModelInfor
            // 
            lbModelInfor.AutoSize = true;
            lbModelInfor.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbModelInfor.Location = new Point(4, 10);
            lbModelInfor.Margin = new Padding(4, 0, 4, 0);
            lbModelInfor.Name = "lbModelInfor";
            lbModelInfor.Size = new Size(1186, 47);
            lbModelInfor.TabIndex = 0;
            lbModelInfor.Text = "Esta es la interfaz para revisar el EBR e imprimir la etiqueta.";
            // 
            // frmTraceAndPrint
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1813, 1050);
            Controls.Add(tbMain);
            Margin = new Padding(4, 5, 4, 5);
            Name = "frmTraceAndPrint";
            Text = "frmTraceAndPrint";
            Load += frmTraceAndPrint_Load;
            tbMain.ResumeLayout(false);
            tbContent.ResumeLayout(false);
            tbMain_Row3.ResumeLayout(false);
            tbData.ResumeLayout(false);
            tbRight.ResumeLayout(false);
            tbRight.PerformLayout();
            pnStatus.ResumeLayout(false);
            panel2.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lbStatus;
        private Button btnPrint;
        private TableLayoutPanel tbMain;
        private TableLayoutPanel tbContent;
        private TableLayoutPanel tbMain_Row3;
        private TableLayoutPanel tbData;
        private Label lbModelInfor;
        private TableLayoutPanel tbRight;
        private Panel pnStatus;
        private RichTextBox rtxtDetailExplain;
        private Panel panel1;
        private TextBox txtScanPid;
        private Panel panel2;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private ListBox lstScanedPids;
        private Button btnClear;
    }
}