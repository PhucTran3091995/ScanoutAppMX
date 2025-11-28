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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
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
            pnStatus = new Panel();
            lbStatus = new Label();
            pnDetailStatus = new Panel();
            lbDetailStatus = new Label();
            btnPrint = new Button();
            panel1 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            rtxtDetailExplain = new RichTextBox();
            tbPreviewLabel = new TableLayoutPanel();
            tbPreview2 = new TableLayoutPanel();
            lbPreviewPno = new Label();
            label8 = new Label();
            tbPreview3 = new TableLayoutPanel();
            lbPreviewScanQty = new Label();
            label7 = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            label4 = new Label();
            tableLayoutPanel3 = new TableLayoutPanel();
            pnPID = new Panel();
            lbPID = new Label();
            cbManualQty = new CheckBox();
            cb6PCBs = new CheckBox();
            cb24PCBs = new CheckBox();
            tbMain = new TableLayoutPanel();
            tbSettingEBR = new TableLayoutPanel();
            txtInspector2 = new TextBox();
            txtInspector1 = new TextBox();
            label5 = new Label();
            label1 = new Label();
            label3 = new Label();
            txtSettingEBR = new TextBox();
            cbtnConfirmSetting = new CheckBox();
            tbContent = new TableLayoutPanel();
            btnSummary = new Button();
            tbMain_Row3.SuspendLayout();
            tbData.SuspendLayout();
            pnRight.SuspendLayout();
            tbChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartProgress).BeginInit();
            pnProgressStatusData.SuspendLayout();
            tbRight.SuspendLayout();
            pnStatus.SuspendLayout();
            pnDetailStatus.SuspendLayout();
            panel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tbPreviewLabel.SuspendLayout();
            tbPreview2.SuspendLayout();
            tbPreview3.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            pnPID.SuspendLayout();
            tbMain.SuspendLayout();
            tbSettingEBR.SuspendLayout();
            tbContent.SuspendLayout();
            SuspendLayout();
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
            tbMain_Row3.RowStyles.Add(new RowStyle(SizeType.Absolute, 1150F));
            tbMain_Row3.Size = new Size(2618, 1150);
            tbMain_Row3.TabIndex = 0;
            // 
            // tbData
            // 
            tbData.ColumnCount = 2;
            tbData.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));
            tbData.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbData.Controls.Add(pnRight, 0, 0);
            tbData.Controls.Add(tbRight, 1, 0);
            tbData.Dock = DockStyle.Fill;
            tbData.Location = new Point(4, 5);
            tbData.Margin = new Padding(4, 5, 4, 5);
            tbData.Name = "tbData";
            tbData.RowCount = 1;
            tbData.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbData.Size = new Size(2610, 1140);
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
            pnRight.Location = new Point(4, 5);
            pnRight.Margin = new Padding(4, 5, 4, 5);
            pnRight.Name = "pnRight";
            pnRight.Size = new Size(392, 1130);
            pnRight.TabIndex = 0;
            // 
            // btnTraceAndPrint
            // 
            btnTraceAndPrint.BackColor = Color.FromArgb(255, 255, 128);
            btnTraceAndPrint.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnTraceAndPrint.Location = new Point(10, 968);
            btnTraceAndPrint.Margin = new Padding(4, 5, 4, 5);
            btnTraceAndPrint.Name = "btnTraceAndPrint";
            btnTraceAndPrint.Size = new Size(371, 155);
            btnTraceAndPrint.TabIndex = 23;
            btnTraceAndPrint.Text = "🕒 Revisar historial e imprimir etiqueta";
            btnTraceAndPrint.UseVisualStyleBackColor = false;
            btnTraceAndPrint.Click += btnTraceAndPrint_Click;
            // 
            // btnClearNotice
            // 
            btnClearNotice.BackColor = SystemColors.ActiveCaption;
            btnClearNotice.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnClearNotice.Location = new Point(9, 803);
            btnClearNotice.Margin = new Padding(4, 5, 4, 5);
            btnClearNotice.Name = "btnClearNotice";
            btnClearNotice.Size = new Size(371, 155);
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
            tbChart.Location = new Point(10, 463);
            tbChart.Margin = new Padding(4, 5, 4, 5);
            tbChart.Name = "tbChart";
            tbChart.RowCount = 2;
            tbChart.RowStyles.Add(new RowStyle(SizeType.Percent, 78.5467148F));
            tbChart.RowStyles.Add(new RowStyle(SizeType.Percent, 21.4532871F));
            tbChart.Size = new Size(370, 330);
            tbChart.TabIndex = 21;
            // 
            // chartProgress
            // 
            chartArea2.Name = "ChartArea1";
            chartProgress.ChartAreas.Add(chartArea2);
            chartProgress.Dock = DockStyle.Fill;
            legend2.Name = "Legend1";
            chartProgress.Legends.Add(legend2);
            chartProgress.Location = new Point(4, 5);
            chartProgress.Margin = new Padding(4, 5, 4, 5);
            chartProgress.Name = "chartProgress";
            chartProgress.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            chartProgress.Series.Add(series2);
            chartProgress.Size = new Size(362, 249);
            chartProgress.TabIndex = 19;
            chartProgress.Text = "chart1";
            // 
            // pnProgressStatusData
            // 
            pnProgressStatusData.Controls.Add(lbProgressData);
            pnProgressStatusData.Dock = DockStyle.Fill;
            pnProgressStatusData.Location = new Point(4, 264);
            pnProgressStatusData.Margin = new Padding(4, 5, 4, 5);
            pnProgressStatusData.Name = "pnProgressStatusData";
            pnProgressStatusData.Size = new Size(362, 61);
            pnProgressStatusData.TabIndex = 20;
            // 
            // lbProgressData
            // 
            lbProgressData.Dock = DockStyle.Fill;
            lbProgressData.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbProgressData.ForeColor = SystemColors.AppWorkspace;
            lbProgressData.Location = new Point(0, 0);
            lbProgressData.Margin = new Padding(4, 0, 4, 0);
            lbProgressData.Name = "lbProgressData";
            lbProgressData.Size = new Size(362, 61);
            lbProgressData.TabIndex = 22;
            lbProgressData.Text = "- / - (-)";
            lbProgressData.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbProgress
            // 
            lbProgress.AutoSize = true;
            lbProgress.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbProgress.ForeColor = SystemColors.AppWorkspace;
            lbProgress.Location = new Point(10, 427);
            lbProgress.Margin = new Padding(4, 0, 4, 0);
            lbProgress.Name = "lbProgress";
            lbProgress.Size = new Size(118, 29);
            lbProgress.TabIndex = 7;
            lbProgress.Text = "Progreso";
            // 
            // lbRemainQty
            // 
            lbRemainQty.AutoSize = true;
            lbRemainQty.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbRemainQty.ForeColor = SystemColors.AppWorkspace;
            lbRemainQty.Location = new Point(10, 342);
            lbRemainQty.Margin = new Padding(4, 0, 4, 0);
            lbRemainQty.Name = "lbRemainQty";
            lbRemainQty.Size = new Size(215, 29);
            lbRemainQty.TabIndex = 18;
            lbRemainQty.Text = "Cantidad restante";
            // 
            // lbBuyer
            // 
            lbBuyer.AutoSize = true;
            lbBuyer.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbBuyer.ForeColor = SystemColors.AppWorkspace;
            lbBuyer.Location = new Point(10, 257);
            lbBuyer.Margin = new Padding(4, 0, 4, 0);
            lbBuyer.Name = "lbBuyer";
            lbBuyer.Size = new Size(142, 29);
            lbBuyer.TabIndex = 17;
            lbBuyer.Text = "Comprador";
            // 
            // lbRemainQtyData
            // 
            lbRemainQtyData.AutoSize = true;
            lbRemainQtyData.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbRemainQtyData.Location = new Point(31, 367);
            lbRemainQtyData.Margin = new Padding(4, 0, 4, 0);
            lbRemainQtyData.Name = "lbRemainQtyData";
            lbRemainQtyData.Size = new Size(34, 47);
            lbRemainQtyData.TabIndex = 10;
            lbRemainQtyData.Text = "-";
            // 
            // lbBuyerData
            // 
            lbBuyerData.AutoSize = true;
            lbBuyerData.Font = new Font("Arial", 14.25F, FontStyle.Bold);
            lbBuyerData.Location = new Point(31, 285);
            lbBuyerData.Margin = new Padding(4, 0, 4, 0);
            lbBuyerData.Name = "lbBuyerData";
            lbBuyerData.Size = new Size(25, 34);
            lbBuyerData.TabIndex = 9;
            lbBuyerData.Text = "-";
            // 
            // lbWoData
            // 
            lbWoData.AutoSize = true;
            lbWoData.Font = new Font("Arial", 14.25F, FontStyle.Bold);
            lbWoData.Location = new Point(31, 202);
            lbWoData.Margin = new Padding(4, 0, 4, 0);
            lbWoData.Name = "lbWoData";
            lbWoData.Size = new Size(25, 34);
            lbWoData.TabIndex = 8;
            lbWoData.Text = "-";
            // 
            // lbModelSuffixData
            // 
            lbModelSuffixData.AutoSize = true;
            lbModelSuffixData.Font = new Font("Arial", 14.25F, FontStyle.Bold);
            lbModelSuffixData.Location = new Point(31, 120);
            lbModelSuffixData.Margin = new Padding(4, 0, 4, 0);
            lbModelSuffixData.Name = "lbModelSuffixData";
            lbModelSuffixData.Size = new Size(25, 34);
            lbModelSuffixData.TabIndex = 4;
            lbModelSuffixData.Text = "-";
            // 
            // lbWO
            // 
            lbWO.AutoSize = true;
            lbWO.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbWO.ForeColor = SystemColors.AppWorkspace;
            lbWO.Location = new Point(10, 172);
            lbWO.Margin = new Padding(4, 0, 4, 0);
            lbWO.Name = "lbWO";
            lbWO.Size = new Size(205, 29);
            lbWO.TabIndex = 2;
            lbWO.Text = "Orden de trabajo";
            // 
            // lbModelSuffix
            // 
            lbModelSuffix.AutoSize = true;
            lbModelSuffix.Font = new Font("Arial", 12F, FontStyle.Bold);
            lbModelSuffix.ForeColor = SystemColors.AppWorkspace;
            lbModelSuffix.Location = new Point(10, 87);
            lbModelSuffix.Margin = new Padding(4, 0, 4, 0);
            lbModelSuffix.Name = "lbModelSuffix";
            lbModelSuffix.Size = new Size(216, 29);
            lbModelSuffix.TabIndex = 1;
            lbModelSuffix.Text = "Sufijo del modelo";
            // 
            // lbModelInfor
            // 
            lbModelInfor.AutoSize = true;
            lbModelInfor.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbModelInfor.Location = new Point(9, 15);
            lbModelInfor.Margin = new Padding(4, 0, 4, 0);
            lbModelInfor.Name = "lbModelInfor";
            lbModelInfor.Size = new Size(253, 47);
            lbModelInfor.TabIndex = 0;
            lbModelInfor.Text = "Información";
            // 
            // tbRight
            // 
            tbRight.ColumnCount = 1;
            tbRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbRight.Controls.Add(pnStatus, 0, 0);
            tbRight.Controls.Add(pnDetailStatus, 0, 1);
            tbRight.Controls.Add(btnPrint, 0, 4);
            tbRight.Controls.Add(panel1, 0, 3);
            tbRight.Controls.Add(tableLayoutPanel3, 0, 2);
            tbRight.Dock = DockStyle.Fill;
            tbRight.Location = new Point(404, 5);
            tbRight.Margin = new Padding(4, 5, 4, 5);
            tbRight.Name = "tbRight";
            tbRight.RowCount = 5;
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 167F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 83F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 83F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 167F));
            tbRight.Size = new Size(2202, 1130);
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
            pnStatus.Size = new Size(2194, 157);
            pnStatus.TabIndex = 0;
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
            lbStatus.Size = new Size(2194, 157);
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
            pnDetailStatus.Location = new Point(4, 172);
            pnDetailStatus.Margin = new Padding(4, 5, 4, 5);
            pnDetailStatus.Name = "pnDetailStatus";
            pnDetailStatus.Size = new Size(2194, 73);
            pnDetailStatus.TabIndex = 1;
            // 
            // lbDetailStatus
            // 
            lbDetailStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lbDetailStatus.AutoSize = true;
            lbDetailStatus.BackColor = Color.Transparent;
            lbDetailStatus.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbDetailStatus.ForeColor = Color.Yellow;
            lbDetailStatus.Location = new Point(9, 8);
            lbDetailStatus.Margin = new Padding(4, 0, 4, 0);
            lbDetailStatus.Name = "lbDetailStatus";
            lbDetailStatus.Size = new Size(31, 43);
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
            btnPrint.Location = new Point(4, 968);
            btnPrint.Margin = new Padding(4, 5, 4, 5);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(2194, 157);
            btnPrint.TabIndex = 2;
            btnPrint.Text = "🖨️ Imprimir Etiqueta Ahora";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(4, 338);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(2194, 620);
            panel1.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 499F));
            tableLayoutPanel1.Controls.Add(rtxtDetailExplain, 0, 0);
            tableLayoutPanel1.Controls.Add(tbPreviewLabel, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 5, 4, 5);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(2194, 620);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // rtxtDetailExplain
            // 
            rtxtDetailExplain.BorderStyle = BorderStyle.FixedSingle;
            rtxtDetailExplain.Dock = DockStyle.Fill;
            rtxtDetailExplain.Location = new Point(4, 5);
            rtxtDetailExplain.Margin = new Padding(4, 5, 4, 5);
            rtxtDetailExplain.Name = "rtxtDetailExplain";
            rtxtDetailExplain.Size = new Size(1687, 610);
            rtxtDetailExplain.TabIndex = 0;
            rtxtDetailExplain.Text = "";
            // 
            // tbPreviewLabel
            // 
            tbPreviewLabel.ColumnCount = 1;
            tbPreviewLabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbPreviewLabel.Controls.Add(tbPreview2, 0, 1);
            tbPreviewLabel.Controls.Add(tbPreview3, 0, 2);
            tbPreviewLabel.Controls.Add(tableLayoutPanel2, 0, 0);
            tbPreviewLabel.Dock = DockStyle.Fill;
            tbPreviewLabel.Location = new Point(1699, 5);
            tbPreviewLabel.Margin = new Padding(4, 5, 4, 5);
            tbPreviewLabel.Name = "tbPreviewLabel";
            tbPreviewLabel.RowCount = 3;
            tbPreviewLabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 67F));
            tbPreviewLabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 83F));
            tbPreviewLabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 335F));
            tbPreviewLabel.Size = new Size(491, 610);
            tbPreviewLabel.TabIndex = 1;
            // 
            // tbPreview2
            // 
            tbPreview2.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tbPreview2.ColumnCount = 2;
            tbPreview2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 143F));
            tbPreview2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbPreview2.Controls.Add(lbPreviewPno, 1, 0);
            tbPreview2.Controls.Add(label8, 0, 0);
            tbPreview2.Dock = DockStyle.Fill;
            tbPreview2.Location = new Point(4, 72);
            tbPreview2.Margin = new Padding(4, 5, 4, 5);
            tbPreview2.Name = "tbPreview2";
            tbPreview2.RowCount = 1;
            tbPreview2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbPreview2.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            tbPreview2.Size = new Size(483, 73);
            tbPreview2.TabIndex = 1;
            // 
            // lbPreviewPno
            // 
            lbPreviewPno.AutoSize = true;
            lbPreviewPno.Dock = DockStyle.Fill;
            lbPreviewPno.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbPreviewPno.Location = new Point(149, 1);
            lbPreviewPno.Margin = new Padding(4, 0, 4, 0);
            lbPreviewPno.Name = "lbPreviewPno";
            lbPreviewPno.Size = new Size(329, 71);
            lbPreviewPno.TabIndex = 5;
            lbPreviewPno.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Dock = DockStyle.Fill;
            label8.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label8.Location = new Point(5, 1);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(135, 71);
            label8.TabIndex = 3;
            label8.Text = "Número de pieza";
            label8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tbPreview3
            // 
            tbPreview3.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tbPreview3.ColumnCount = 2;
            tbPreview3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 143F));
            tbPreview3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbPreview3.Controls.Add(lbPreviewScanQty, 1, 0);
            tbPreview3.Controls.Add(label7, 0, 0);
            tbPreview3.Dock = DockStyle.Fill;
            tbPreview3.Location = new Point(4, 155);
            tbPreview3.Margin = new Padding(4, 5, 4, 5);
            tbPreview3.Name = "tbPreview3";
            tbPreview3.RowCount = 1;
            tbPreview3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbPreview3.Size = new Size(483, 450);
            tbPreview3.TabIndex = 2;
            // 
            // lbPreviewScanQty
            // 
            lbPreviewScanQty.AutoSize = true;
            lbPreviewScanQty.Dock = DockStyle.Fill;
            lbPreviewScanQty.Font = new Font("Arial", 72F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbPreviewScanQty.Location = new Point(149, 1);
            lbPreviewScanQty.Margin = new Padding(4, 0, 4, 0);
            lbPreviewScanQty.Name = "lbPreviewScanQty";
            lbPreviewScanQty.Size = new Size(329, 448);
            lbPreviewScanQty.TabIndex = 5;
            lbPreviewScanQty.Text = "0";
            lbPreviewScanQty.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Dock = DockStyle.Fill;
            label7.Font = new Font("Arial Narrow", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(5, 1);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(135, 448);
            label7.TabIndex = 4;
            label7.Text = "Número de OK escaneados";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(label4, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(4, 5);
            tableLayoutPanel2.Margin = new Padding(4, 5, 4, 5);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(483, 57);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(5, 1);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(473, 55);
            label4.TabIndex = 2;
            label4.Text = "Vista previa de la etiqueta";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel3.ColumnCount = 5;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 77.12984F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.701594F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 121F));
            tableLayoutPanel3.Controls.Add(pnPID, 0, 0);
            tableLayoutPanel3.Controls.Add(cbManualQty, 1, 0);
            tableLayoutPanel3.Controls.Add(cb6PCBs, 2, 0);
            tableLayoutPanel3.Controls.Add(cb24PCBs, 3, 0);
            tableLayoutPanel3.Controls.Add(btnSummary, 4, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 253);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(2196, 77);
            tableLayoutPanel3.TabIndex = 5;
            // 
            // pnPID
            // 
            pnPID.BackColor = SystemColors.ButtonFace;
            pnPID.Controls.Add(lbPID);
            pnPID.Dock = DockStyle.Fill;
            pnPID.Location = new Point(5, 6);
            pnPID.Margin = new Padding(4, 5, 4, 5);
            pnPID.Name = "pnPID";
            pnPID.Size = new Size(1590, 65);
            pnPID.TabIndex = 3;
            // 
            // lbPID
            // 
            lbPID.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lbPID.AutoSize = true;
            lbPID.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbPID.ForeColor = SystemColors.ActiveCaptionText;
            lbPID.Location = new Point(9, 12);
            lbPID.Margin = new Padding(4, 0, 4, 0);
            lbPID.Name = "lbPID";
            lbPID.Size = new Size(189, 43);
            lbPID.TabIndex = 3;
            lbPID.Text = "PCBA S/N";
            // 
            // cbManualQty
            // 
            cbManualQty.Anchor = AnchorStyles.None;
            cbManualQty.AutoSize = true;
            cbManualQty.Location = new Point(1642, 24);
            cbManualQty.Name = "cbManualQty";
            cbManualQty.Size = new Size(96, 29);
            cbManualQty.TabIndex = 4;
            cbManualQty.Text = "Manual";
            cbManualQty.UseVisualStyleBackColor = true;
            // 
            // cb6PCBs
            // 
            cb6PCBs.Anchor = AnchorStyles.None;
            cb6PCBs.AutoSize = true;
            cb6PCBs.Location = new Point(1805, 24);
            cb6PCBs.Name = "cb6PCBs";
            cb6PCBs.Size = new Size(97, 29);
            cb6PCBs.TabIndex = 5;
            cb6PCBs.Text = "6 PCBs ";
            cb6PCBs.UseVisualStyleBackColor = true;
            // 
            // cb24PCBs
            // 
            cb24PCBs.Anchor = AnchorStyles.None;
            cb24PCBs.AutoSize = true;
            cb24PCBs.Location = new Point(1948, 24);
            cb24PCBs.Name = "cb24PCBs";
            cb24PCBs.Size = new Size(102, 29);
            cb24PCBs.TabIndex = 6;
            cb24PCBs.Text = "24 PCBs";
            cb24PCBs.UseVisualStyleBackColor = true;
            // 
            // tbMain
            // 
            tbMain.ColumnCount = 1;
            tbMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbMain.Controls.Add(tbSettingEBR, 0, 0);
            tbMain.Controls.Add(tbContent, 0, 1);
            tbMain.Dock = DockStyle.Fill;
            tbMain.Location = new Point(0, 0);
            tbMain.Margin = new Padding(4, 5, 4, 5);
            tbMain.Name = "tbMain";
            tbMain.RowCount = 2;
            tbMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 83F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tbMain.Size = new Size(2634, 1253);
            tbMain.TabIndex = 1;
            // 
            // tbSettingEBR
            // 
            tbSettingEBR.BackColor = SystemColors.Info;
            tbSettingEBR.ColumnCount = 7;
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 151F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 287F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 207F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 139F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 253F));
            tbSettingEBR.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tbSettingEBR.Controls.Add(txtInspector2, 6, 0);
            tbSettingEBR.Controls.Add(txtInspector1, 4, 0);
            tbSettingEBR.Controls.Add(label5, 5, 0);
            tbSettingEBR.Controls.Add(label1, 3, 0);
            tbSettingEBR.Controls.Add(label3, 0, 0);
            tbSettingEBR.Controls.Add(txtSettingEBR, 1, 0);
            tbSettingEBR.Controls.Add(cbtnConfirmSetting, 2, 0);
            tbSettingEBR.Dock = DockStyle.Fill;
            tbSettingEBR.Location = new Point(4, 5);
            tbSettingEBR.Margin = new Padding(4, 5, 4, 5);
            tbSettingEBR.Name = "tbSettingEBR";
            tbSettingEBR.RowCount = 1;
            tbSettingEBR.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbSettingEBR.Size = new Size(2626, 73);
            tbSettingEBR.TabIndex = 2;
            // 
            // txtInspector2
            // 
            txtInspector2.Dock = DockStyle.Fill;
            txtInspector2.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtInspector2.Location = new Point(1835, 5);
            txtInspector2.Margin = new Padding(4, 5, 4, 5);
            txtInspector2.MaxLength = 45;
            txtInspector2.Name = "txtInspector2";
            txtInspector2.Size = new Size(787, 54);
            txtInspector2.TabIndex = 9;
            // 
            // txtInspector1
            // 
            txtInspector1.Dock = DockStyle.Fill;
            txtInspector1.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtInspector1.Location = new Point(788, 5);
            txtInspector1.Margin = new Padding(4, 5, 4, 5);
            txtInspector1.MaxLength = 45;
            txtInspector1.Name = "txtInspector1";
            txtInspector1.Size = new Size(786, 54);
            txtInspector1.TabIndex = 8;
            // 
            // label5
            // 
            label5.Dock = DockStyle.Fill;
            label5.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(1582, 0);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(245, 73);
            label5.TabIndex = 7;
            label5.Text = "Segundo inspector";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(649, 0);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(131, 73);
            label1.TabIndex = 5;
            label1.Text = "Primer inspector";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(4, 0);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(143, 73);
            label3.TabIndex = 2;
            label3.Text = "Trabajando con código";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSettingEBR
            // 
            txtSettingEBR.Dock = DockStyle.Fill;
            txtSettingEBR.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtSettingEBR.Location = new Point(155, 5);
            txtSettingEBR.Margin = new Padding(4, 5, 4, 5);
            txtSettingEBR.Name = "txtSettingEBR";
            txtSettingEBR.Size = new Size(279, 54);
            txtSettingEBR.TabIndex = 3;
            // 
            // cbtnConfirmSetting
            // 
            cbtnConfirmSetting.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            cbtnConfirmSetting.AutoSize = true;
            cbtnConfirmSetting.Font = new Font("Arial", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            cbtnConfirmSetting.Location = new Point(442, 5);
            cbtnConfirmSetting.Margin = new Padding(4, 5, 4, 5);
            cbtnConfirmSetting.Name = "cbtnConfirmSetting";
            cbtnConfirmSetting.Size = new Size(197, 63);
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
            tbContent.Location = new Point(4, 88);
            tbContent.Margin = new Padding(4, 5, 4, 5);
            tbContent.Name = "tbContent";
            tbContent.RowCount = 1;
            tbContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tbContent.Size = new Size(2626, 1160);
            tbContent.TabIndex = 3;
            // 
            // btnSummary
            // 
            btnSummary.Anchor = AnchorStyles.None;
            btnSummary.Location = new Point(2084, 14);
            btnSummary.Name = "btnSummary";
            btnSummary.Size = new Size(99, 49);
            btnSummary.TabIndex = 7;
            btnSummary.Text = "Summary";
            btnSummary.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2634, 1253);
            Controls.Add(tbMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            Name = "Form1";
            Text = "[VR-HS] Línea PCBA OSP para VR HAENGSUNG · Embalaje";
            Load += Form1_Load;
            KeyDown += Form1_KeyDown;
            tbMain_Row3.ResumeLayout(false);
            tbData.ResumeLayout(false);
            pnRight.ResumeLayout(false);
            pnRight.PerformLayout();
            tbChart.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chartProgress).EndInit();
            pnProgressStatusData.ResumeLayout(false);
            tbRight.ResumeLayout(false);
            pnStatus.ResumeLayout(false);
            pnDetailStatus.ResumeLayout(false);
            pnDetailStatus.PerformLayout();
            panel1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tbPreviewLabel.ResumeLayout(false);
            tbPreview2.ResumeLayout(false);
            tbPreview2.PerformLayout();
            tbPreview3.ResumeLayout(false);
            tbPreview3.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            pnPID.ResumeLayout(false);
            pnPID.PerformLayout();
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
        private Button btnClearNotice;
        private Button btnTraceAndPrint;
        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel1;
        private RichTextBox rtxtDetailExplain;
        private TableLayoutPanel tbPreviewLabel;
        private TableLayoutPanel tableLayoutPanel3;
        private Label label8;
        private TableLayoutPanel tableLayoutPanel4;
        private Label label7;
        private Label lbPreviewPno;
        private Label lbPreviewScanQty;
        private TableLayoutPanel tbPreview2;
        private TableLayoutPanel tbPreview3;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label4;
        private CheckBox cbManualQty;
        private CheckBox cb6PCBs;
        private CheckBox cb24PCBs;
        private Button btnSummary;
    }
}
