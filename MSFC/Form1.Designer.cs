namespace MSFC
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tbMain_Row3 = new TableLayoutPanel();
            tbData = new TableLayoutPanel();
            pnRight = new Panel();
            btnTraceAndPrint = new Button();
            btnClearNotice = new Button();
            tbChart = new TableLayoutPanel();
            chartProgress = new System.Windows.Forms.DataVisualization.Charting.Chart();
            pnProgressStatusData = new Panel();
            lbProgressData = new Label();
            lbProgress = new Label();
            lbRemainQty = new Label();
            lbBuyer = new Label();
            lbRemainQtyData = new Label();
            lbBuyerData = new Label();
            lbWoData = new Label();
            lbModelSuffixData = new Label();
            lbWO = new Label();
            lbModelSuffix = new Label();
            lbModelInfor = new Label();
            tbRight = new TableLayoutPanel();
            pnPID = new Panel();
            lbPID = new Label();
            pnStatus = new Panel();
            lbStatus = new Label();
            pnDetailStatus = new Panel();
            lbDetailStatus = new Label();
            btnPrint = new Button();
            rtxtDetailExplain = new RichTextBox();
            tbMain = new TableLayoutPanel();
            label2 = new Label();
            tbSettingEBR = new TableLayoutPanel();
            txtInspector2 = new TextBox();
            txtInspector1 = new TextBox();
            label5 = new Label();
            label1 = new Label();
            label3 = new Label();
            txtSettingEBR = new TextBox();
            cbtnConfirmSetting = new CheckBox();
            tbContent = new TableLayoutPanel();
            richTextBox1 = new RichTextBox();
            tbMain_Row3.SuspendLayout();
            tbData.SuspendLayout();
            pnRight.SuspendLayout();
            tbChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartProgress).BeginInit();
            pnProgressStatusData.SuspendLayout();
            tbRight.SuspendLayout();
            pnPID.SuspendLayout();
            pnStatus.SuspendLayout();
            pnDetailStatus.SuspendLayout();
            tbMain.SuspendLayout();
            tbSettingEBR.SuspendLayout();
            tbContent.SuspendLayout();
            SuspendLayout();
            // 
            // tbMain_Row3
            // 
            tbMain_Row3.ColumnCount = 1;
            tbMain_Row3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbMain_Row3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tbMain_Row3.Controls.Add(tbData, 0, 0);
            tbMain_Row3.Dock = DockStyle.Fill;
            tbMain_Row3.Location = new Point(3, 3);
            tbMain_Row3.Name = "tbMain_Row3";
            tbMain_Row3.RowCount = 1;
            tbMain_Row3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbMain_Row3.RowStyles.Add(new RowStyle(SizeType.Absolute, 622F));
            tbMain_Row3.Size = new Size(1435, 690);
            tbMain_Row3.TabIndex = 0;
            // 
            // tbData
            // 
            tbData.ColumnCount = 2;
            tbData.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            tbData.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbData.Controls.Add(pnRight, 0, 0);
            tbData.Controls.Add(tbRight, 1, 0);
            tbData.Dock = DockStyle.Fill;
            tbData.Location = new Point(3, 3);
            tbData.Name = "tbData";
            tbData.RowCount = 1;
            tbData.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbData.Size = new Size(1429, 684);
            tbData.TabIndex = 1;
            // 
            // pnRight
            // 
            pnRight.BackColor = SystemColors.ButtonHighlight;
            pnRight.Controls.Add(btnTraceAndPrint);
            pnRight.Controls.Add(btnClearNotice);
            pnRight.Controls.Add(tbChart);
            pnRight.Controls.Add(lbProgress);
            pnRight.Controls.Add(lbRemainQty);
            pnRight.Controls.Add(lbBuyer);
            pnRight.Controls.Add(lbRemainQtyData);
            pnRight.Controls.Add(lbBuyerData);
            pnRight.Controls.Add(lbWoData);
            pnRight.Controls.Add(lbModelSuffixData);
            pnRight.Controls.Add(lbWO);
            pnRight.Controls.Add(lbModelSuffix);
            pnRight.Controls.Add(lbModelInfor);
            pnRight.Dock = DockStyle.Fill;
            pnRight.Location = new Point(3, 3);
            pnRight.Name = "pnRight";
            pnRight.Size = new Size(274, 678);
            pnRight.TabIndex = 0;
            // 
            // btnTraceAndPrint
            // 
            btnTraceAndPrint.BackColor = Color.FromArgb(255, 255, 128);
            btnTraceAndPrint.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnTraceAndPrint.Location = new Point(7, 581);
            btnTraceAndPrint.Name = "btnTraceAndPrint";
            btnTraceAndPrint.Size = new Size(260, 93);
            btnTraceAndPrint.TabIndex = 23;
            btnTraceAndPrint.Text = "🕒 Revisar historial e imprimir etiqueta";
            btnTraceAndPrint.UseVisualStyleBackColor = false;
            btnTraceAndPrint.Click += btnTraceAndPrint_Click;
            // 
            // btnClearNotice
            // 
            btnClearNotice.BackColor = SystemColors.ActiveCaption;
            btnClearNotice.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnClearNotice.Location = new Point(6, 482);
            btnClearNotice.Name = "btnClearNotice";
            btnClearNotice.Size = new Size(260, 93);
            btnClearNotice.TabIndex = 22;
            btnClearNotice.Text = "Restablecer valores predeterminados";
            btnClearNotice.UseVisualStyleBackColor = false;
            btnClearNotice.Click += btnClearNotice_Click;
            // 
            // tbChart
            // 
            tbChart.ColumnCount = 1;
            tbChart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tbChart.Controls.Add(chartProgress, 0, 0);
            tbChart.Controls.Add(pnProgressStatusData, 0, 1);
            tbChart.Location = new Point(7, 278);
            tbChart.Name = "tbChart";
            tbChart.RowCount = 2;
            tbChart.RowStyles.Add(new RowStyle(SizeType.Percent, 78.5467148F));
            tbChart.RowStyles.Add(new RowStyle(SizeType.Percent, 21.4532871F));
            tbChart.Size = new Size(259, 198);
            tbChart.TabIndex = 21;
            // 
            // chartProgress
            // 
            chartArea1.Name = "ChartArea1";
            chartProgress.ChartAreas.Add(chartArea1);
            chartProgress.Dock = DockStyle.Fill;
            legend1.Name = "Legend1";
            chartProgress.Legends.Add(legend1);
            chartProgress.Location = new Point(3, 3);
            chartProgress.Name = "chartProgress";
            chartProgress.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chartProgress.Series.Add(series1);
            chartProgress.Size = new Size(253, 149);
            chartProgress.TabIndex = 19;
            chartProgress.Text = "chart1";
            // 
            // pnProgressStatusData
            // 
            pnProgressStatusData.Controls.Add(lbProgressData);
            pnProgressStatusData.Dock = DockStyle.Fill;
            pnProgressStatusData.Location = new Point(3, 158);
            pnProgressStatusData.Name = "pnProgressStatusData";
            pnProgressStatusData.Size = new Size(253, 37);
            pnProgressStatusData.TabIndex = 20;
            // 
            // lbProgressData
            // 
            lbProgressData.Dock = DockStyle.Fill;
            lbProgressData.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbProgressData.ForeColor = SystemColors.AppWorkspace;
            lbProgressData.Location = new Point(0, 0);
            lbProgressData.Name = "lbProgressData";
            lbProgressData.Size = new Size(253, 37);
            lbProgressData.TabIndex = 22;
            lbProgressData.Text = "- / - (-)";
            lbProgressData.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbProgress
            // 
            lbProgress.AutoSize = true;
            lbProgress.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbProgress.ForeColor = SystemColors.AppWorkspace;
            lbProgress.Location = new Point(7, 256);
            lbProgress.Name = "lbProgress";
            lbProgress.Size = new Size(80, 19);
            lbProgress.TabIndex = 7;
            lbProgress.Text = "Progreso";
            // 
            // lbRemainQty
            // 
            lbRemainQty.AutoSize = true;
            lbRemainQty.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbRemainQty.ForeColor = SystemColors.AppWorkspace;
            lbRemainQty.Location = new Point(7, 205);
            lbRemainQty.Name = "lbRemainQty";
            lbRemainQty.Size = new Size(144, 19);
            lbRemainQty.TabIndex = 18;
            lbRemainQty.Text = "Cantidad restante";
            // 
            // lbBuyer
            // 
            lbBuyer.AutoSize = true;
            lbBuyer.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbBuyer.ForeColor = SystemColors.AppWorkspace;
            lbBuyer.Location = new Point(7, 154);
            lbBuyer.Name = "lbBuyer";
            lbBuyer.Size = new Size(96, 19);
            lbBuyer.TabIndex = 17;
            lbBuyer.Text = "Comprador";
            // 
            // lbRemainQtyData
            // 
            lbRemainQtyData.AutoSize = true;
            lbRemainQtyData.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbRemainQtyData.Location = new Point(22, 220);
            lbRemainQtyData.Name = "lbRemainQtyData";
            lbRemainQtyData.Size = new Size(23, 32);
            lbRemainQtyData.TabIndex = 10;
            lbRemainQtyData.Text = "-";
            // 
            // lbBuyerData
            // 
            lbBuyerData.AutoSize = true;
            lbBuyerData.Font = new Font("Arial", 14.25F, FontStyle.Bold);
            lbBuyerData.Location = new Point(22, 171);
            lbBuyerData.Name = "lbBuyerData";
            lbBuyerData.Size = new Size(16, 22);
            lbBuyerData.TabIndex = 9;
            lbBuyerData.Text = "-";
            // 
            // lbWoData
            // 
            lbWoData.AutoSize = true;
            lbWoData.Font = new Font("Arial", 14.25F, FontStyle.Bold);
            lbWoData.Location = new Point(22, 121);
            lbWoData.Name = "lbWoData";
            lbWoData.Size = new Size(16, 22);
            lbWoData.TabIndex = 8;
            lbWoData.Text = "-";
            // 
            // lbModelSuffixData
            // 
            lbModelSuffixData.AutoSize = true;
            lbModelSuffixData.Font = new Font("Arial", 14.25F, FontStyle.Bold);
            lbModelSuffixData.Location = new Point(22, 72);
            lbModelSuffixData.Name = "lbModelSuffixData";
            lbModelSuffixData.Size = new Size(16, 22);
            lbModelSuffixData.TabIndex = 4;
            lbModelSuffixData.Text = "-";
            // 
            // lbWO
            // 
            lbWO.AutoSize = true;
            lbWO.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbWO.ForeColor = SystemColors.AppWorkspace;
            lbWO.Location = new Point(7, 103);
            lbWO.Name = "lbWO";
            lbWO.Size = new Size(136, 19);
            lbWO.TabIndex = 2;
            lbWO.Text = "Orden de trabajo";
            // 
            // lbModelSuffix
            // 
            lbModelSuffix.AutoSize = true;
            lbModelSuffix.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbModelSuffix.ForeColor = SystemColors.AppWorkspace;
            lbModelSuffix.Location = new Point(7, 52);
            lbModelSuffix.Name = "lbModelSuffix";
            lbModelSuffix.Size = new Size(141, 19);
            lbModelSuffix.TabIndex = 1;
            lbModelSuffix.Text = "Sufijo del modelo";
            // 
            // lbModelInfor
            // 
            lbModelInfor.AutoSize = true;
            lbModelInfor.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbModelInfor.Location = new Point(6, 9);
            lbModelInfor.Name = "lbModelInfor";
            lbModelInfor.Size = new Size(174, 32);
            lbModelInfor.TabIndex = 0;
            lbModelInfor.Text = "Información";
            // 
            // tbRight
            // 
            tbRight.ColumnCount = 1;
            tbRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbRight.Controls.Add(pnPID, 0, 2);
            tbRight.Controls.Add(pnStatus, 0, 0);
            tbRight.Controls.Add(pnDetailStatus, 0, 1);
            tbRight.Controls.Add(btnPrint, 0, 4);
            tbRight.Controls.Add(rtxtDetailExplain, 0, 3);
            tbRight.Dock = DockStyle.Fill;
            tbRight.Location = new Point(283, 3);
            tbRight.Name = "tbRight";
            tbRight.RowCount = 5;
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tbRight.Size = new Size(1143, 678);
            tbRight.TabIndex = 1;
            // 
            // pnPID
            // 
            pnPID.BackColor = SystemColors.ButtonFace;
            pnPID.Controls.Add(lbPID);
            pnPID.Dock = DockStyle.Fill;
            pnPID.Location = new Point(3, 153);
            pnPID.Name = "pnPID";
            pnPID.Size = new Size(1137, 44);
            pnPID.TabIndex = 3;
            // 
            // lbPID
            // 
            lbPID.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lbPID.AutoSize = true;
            lbPID.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbPID.ForeColor = SystemColors.ActiveCaptionText;
            lbPID.Location = new Point(6, 7);
            lbPID.Name = "lbPID";
            lbPID.Size = new Size(126, 29);
            lbPID.TabIndex = 3;
            lbPID.Text = "PCBA S/N";
            // 
            // pnStatus
            // 
            pnStatus.BackColor = Color.Black;
            pnStatus.Controls.Add(lbStatus);
            pnStatus.Dock = DockStyle.Fill;
            pnStatus.Location = new Point(3, 3);
            pnStatus.Name = "pnStatus";
            pnStatus.Size = new Size(1137, 94);
            pnStatus.TabIndex = 0;
            // 
            // lbStatus
            // 
            lbStatus.BackColor = Color.Transparent;
            lbStatus.Dock = DockStyle.Fill;
            lbStatus.Font = new Font("Bookman Old Style", 48F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbStatus.ForeColor = Color.Yellow;
            lbStatus.Location = new Point(0, 0);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new Size(1137, 94);
            lbStatus.TabIndex = 1;
            lbStatus.Text = "----";
            lbStatus.TextAlign = ContentAlignment.MiddleCenter;
            lbStatus.Click += lbStatus_Click;
            // 
            // pnDetailStatus
            // 
            pnDetailStatus.BackColor = SystemColors.ActiveCaptionText;
            pnDetailStatus.Controls.Add(lbDetailStatus);
            pnDetailStatus.Dock = DockStyle.Fill;
            pnDetailStatus.Location = new Point(3, 103);
            pnDetailStatus.Name = "pnDetailStatus";
            pnDetailStatus.Size = new Size(1137, 44);
            pnDetailStatus.TabIndex = 1;
            // 
            // lbDetailStatus
            // 
            lbDetailStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lbDetailStatus.AutoSize = true;
            lbDetailStatus.BackColor = Color.Transparent;
            lbDetailStatus.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbDetailStatus.ForeColor = Color.Yellow;
            lbDetailStatus.Location = new Point(6, 5);
            lbDetailStatus.Name = "lbDetailStatus";
            lbDetailStatus.Size = new Size(21, 29);
            lbDetailStatus.TabIndex = 2;
            lbDetailStatus.Text = "-";
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
            btnPrint.Location = new Point(3, 581);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(1137, 94);
            btnPrint.TabIndex = 2;
            btnPrint.Text = "🖨️ Imprimir Etiqueta Ahora";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // rtxtDetailExplain
            // 
            rtxtDetailExplain.Dock = DockStyle.Fill;
            rtxtDetailExplain.Enabled = false;
            rtxtDetailExplain.Location = new Point(3, 203);
            rtxtDetailExplain.Name = "rtxtDetailExplain";
            rtxtDetailExplain.Size = new Size(1137, 372);
            rtxtDetailExplain.TabIndex = 4;
            rtxtDetailExplain.Text = "";
            // 
            // tbMain
            // 
            tbMain.ColumnCount = 2;
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 397F));
            tbMain.Controls.Add(label2, 1, 0);
            tbMain.Controls.Add(tbSettingEBR, 0, 0);
            tbMain.Controls.Add(tbContent, 0, 1);
            tbMain.Controls.Add(richTextBox1, 1, 1);
            tbMain.Dock = DockStyle.Fill;
            tbMain.Location = new Point(0, 0);
            tbMain.Name = "tbMain";
            tbMain.RowCount = 2;
            tbMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tbMain.Size = new Size(1844, 752);
            tbMain.TabIndex = 1;
            // 
            // label2
            // 
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(1450, 0);
            label2.Name = "label2";
            label2.Size = new Size(391, 50);
            label2.TabIndex = 5;
            label2.Text = "Log";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tbSettingEBR
            // 
            tbSettingEBR.BackColor = SystemColors.Info;
            tbSettingEBR.ColumnCount = 7;
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 106F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 201F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 97F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 97F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tbSettingEBR.Controls.Add(txtInspector2, 6, 0);
            tbSettingEBR.Controls.Add(txtInspector1, 4, 0);
            tbSettingEBR.Controls.Add(label5, 5, 0);
            tbSettingEBR.Controls.Add(label1, 3, 0);
            tbSettingEBR.Controls.Add(label3, 0, 0);
            tbSettingEBR.Controls.Add(txtSettingEBR, 1, 0);
            tbSettingEBR.Controls.Add(cbtnConfirmSetting, 2, 0);
            tbSettingEBR.Dock = DockStyle.Fill;
            tbSettingEBR.Location = new Point(3, 3);
            tbSettingEBR.Name = "tbSettingEBR";
            tbSettingEBR.RowCount = 1;
            tbSettingEBR.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbSettingEBR.Size = new Size(1441, 44);
            tbSettingEBR.TabIndex = 2;
            // 
            // txtInspector2
            // 
            txtInspector2.Dock = DockStyle.Fill;
            txtInspector2.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtInspector2.Location = new Point(1046, 3);
            txtInspector2.MaxLength = 45;
            txtInspector2.Name = "txtInspector2";
            txtInspector2.Size = new Size(392, 39);
            txtInspector2.TabIndex = 9;
            // 
            // txtInspector1
            // 
            txtInspector1.Dock = DockStyle.Fill;
            txtInspector1.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtInspector1.Location = new Point(552, 3);
            txtInspector1.MaxLength = 45;
            txtInspector1.Name = "txtInspector1";
            txtInspector1.Size = new Size(391, 39);
            txtInspector1.TabIndex = 8;
            // 
            // label5
            // 
            label5.Dock = DockStyle.Fill;
            label5.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(949, 0);
            label5.Name = "label5";
            label5.Size = new Size(91, 44);
            label5.TabIndex = 7;
            label5.Text = "Segundo inspector";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(455, 0);
            label1.Name = "label1";
            label1.Size = new Size(91, 44);
            label1.TabIndex = 5;
            label1.Text = "Primer inspector";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(3, 0);
            label3.Name = "label3";
            label3.Size = new Size(100, 44);
            label3.TabIndex = 2;
            label3.Text = "Trabajando con código";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSettingEBR
            // 
            txtSettingEBR.Dock = DockStyle.Fill;
            txtSettingEBR.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtSettingEBR.Location = new Point(109, 3);
            txtSettingEBR.Name = "txtSettingEBR";
            txtSettingEBR.Size = new Size(195, 39);
            txtSettingEBR.TabIndex = 3;
            // 
            // cbtnConfirmSetting
            // 
            cbtnConfirmSetting.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            cbtnConfirmSetting.AutoSize = true;
            cbtnConfirmSetting.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            cbtnConfirmSetting.Location = new Point(310, 3);
            cbtnConfirmSetting.Name = "cbtnConfirmSetting";
            cbtnConfirmSetting.Size = new Size(129, 38);
            cbtnConfirmSetting.TabIndex = 4;
            cbtnConfirmSetting.Text = "Confirmar";
            cbtnConfirmSetting.UseVisualStyleBackColor = true;
            cbtnConfirmSetting.CheckedChanged += cbtnConfirmSetting_CheckedChanged;
            // 
            // tbContent
            // 
            tbContent.ColumnCount = 1;
            tbContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbContent.Controls.Add(tbMain_Row3, 0, 0);
            tbContent.Dock = DockStyle.Fill;
            tbContent.Location = new Point(3, 53);
            tbContent.Name = "tbContent";
            tbContent.RowCount = 1;
            tbContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbContent.Size = new Size(1441, 696);
            tbContent.TabIndex = 3;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(1450, 53);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(391, 696);
            richTextBox1.TabIndex = 4;
            richTextBox1.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1844, 752);
            Controls.Add(tbMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "[VR-HS] Línea PCBA OSP para VR HAENGSUNG · Embalaje";
            Load += Form1_Load;
            tbMain_Row3.ResumeLayout(false);
            tbData.ResumeLayout(false);
            pnRight.ResumeLayout(false);
            pnRight.PerformLayout();
            tbChart.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chartProgress).EndInit();
            pnProgressStatusData.ResumeLayout(false);
            tbRight.ResumeLayout(false);
            pnPID.ResumeLayout(false);
            pnPID.PerformLayout();
            pnStatus.ResumeLayout(false);
            pnDetailStatus.ResumeLayout(false);
            pnDetailStatus.PerformLayout();
            tbMain.ResumeLayout(false);
            tbSettingEBR.ResumeLayout(false);
            tbSettingEBR.PerformLayout();
            tbContent.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tbMain_Row3;
        private TableLayoutPanel tbData;
        private Panel pnRight;
        private Label label8;
        private Label lbRemainQtyData;
        private Label lbBuyerData;
        private Label lbWoData;
        private Label lbProgress;
        private Label lbModelSuffixData;
        private Label lbWO;
        private Label lbModelSuffix;
        private Label lbModelInfor;
        private Label lbRemainQty;
        private Label lbBuyer;
        private TableLayoutPanel tbRight;
        private Panel pnStatus;
        private Panel pnDetailStatus;
        private Button btnPrint;
        private Panel pnPID;
        private Label lbStatus;
        private Label lbDetailStatus;
        private Label lbPID;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartProgress;
        private TableLayoutPanel tbChart;
        private Panel pnProgressStatusData;
        private Label lbProgressData;
        private TableLayoutPanel tbMain;
        private TableLayoutPanel tbSettingEBR;
        private Label label3;
        private TextBox txtSettingEBR;
        private CheckBox cbtnConfirmSetting;
        private TableLayoutPanel tbContent;
        private TextBox txtInspector2;
        private TextBox txtInspector1;
        private Label label5;
        private Label label1;
        private RichTextBox rtxtDetailExplain;
        private Button btnClearNotice;
        private Button btnTraceAndPrint;
        private Label label2;
        private RichTextBox richTextBox1;
    }
}
