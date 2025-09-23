using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;   // for TreeScope, ControlType, etc.
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Patterns;             // ValuePattern (for ValueProperty)
using FlaUI.UIA3;
using FlaUI.UIA3.Patterns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MSFC.Data;
using MSFC.Dto;
using MSFC.Models;
using MSFC.UC;
using ScanOutTool.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static ZXing.QrCode.Internal.Version;



namespace MSFC
{
    public partial class Form1 : Form
    {
        private readonly IDbContextFactory<MMesDbContext> _dbFactory;
        private readonly IConfiguration _config;
        private SerialPort _portScanner;
        private SerialPort _portSFC;
        private PrintDocument printDoc = new PrintDocument();
        private ManufacturingTagDto tagLabel = new ManufacturingTagDto();
        private string hostName = Dns.GetHostName();
        private string ip;
        string CurrentPid;

        private readonly IAutoScanOutUI _ui;

        public Form1(IDbContextFactory<MMesDbContext> dbFactory, IConfiguration config/*, IAutoScanOutUI ui*/)
        {
            _dbFactory = dbFactory;
            _config = config;
            //_ui = ui;

            InitializeComponent();

            // Setup Chart
            SetupDonut(chartProgress);

            // Setup COM
            SetupPortScanner();
            SetupPortSFC();


            // Printer
            printDoc.PrintPage += PrintDoc_PrintPage;

            // Get IP of client
            ip = Dns.GetHostAddresses(hostName)
                       .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                       ?.ToString();

            //// Subscribe UI Automation
            //_ui.PIDChanged += UiOnPidChanged;
            //SafeSetText(slbLog, _ui.ReadPID());





        }
        private async Task UiOnPidChanged(string newPid)
        {
            // Event có thể đến từ thread khác → marshal về UI thread
            if (InvokeRequired) { BeginInvoke(new Action(async () => await UiOnPidChanged(newPid))); return; }

            slbLog.Text = $"PID: {newPid}";


            PCB pCB = new PCB()
            {
                PID = _ui.ReadPID(),
                EBR = _ui.ReadEBR(),
                WO = _ui.ReadWO(),
                Result = _ui.ReadResult(),
                Message = _ui.ReadMessage(),
                Progress = _ui.ReadProgress(),
            };

            await UpdateUI(pCB);

        }

        private async Task UpdateUI(PCB data)
        {
            if (txtSettingEBR.Text == "")
            {
                txtSettingEBR.Text = _ui.ReadEBR();
            }

            int TotalQty = ExtractProgress(data.Progress).WoQty;
            int CompletedQty = ExtractProgress(data.Progress).CompletedQty;
            int RemainQty = TotalQty - CompletedQty;

            // Left panel
            lbModelSuffixData.Text = data.EBR;
            lbWoData.Text = data.WO;
            lbBuyerData.Text = data.Buyer;

            // Right panel
            lbStatus.Text = data.Result;
            lbDetailStatus.Text = data.Message;
            lbPID.Text = $"PCBA S/N: {data.PID}";

            if (data.Result.Equals("OK", StringComparison.Ordinal))
            {

                lbStatus.ForeColor = Color.Green;

                lbRemainQtyData.Text = RemainQty.ToString();
                SetWorkOrderProgress(chartProgress, lbProgressData, completed: CompletedQty, total: TotalQty);

                UcSlot ucSlot = new UcSlot();
                ucSlot.lbWO.Text = data.WO;
                ucSlot.lbPID.Text = GetLast11(data.PID);
                ucSlot.lbProgress.Text = data.Progress;
                ucSlot.lbEBR.Text = data.EBR;
                fpnScanData.Controls.Add(ucSlot);

                // Upload data to DB
                var section = _config.GetSection("TagSetting");
                string _4M = section["4M"] ?? "";
                var scanOutData = new TbScanOut()
                {
                    ClientId = ip,
                    Pid = data.PID,
                    PartNo = data.EBR,
                    WorkOrder = data.WO,
                    ScanAt = DateTime.Now,
                    Qty = 1,
                    FirstInspector = txt1stInspetor.Text,
                    SecondInspector = txt2ndInspector.Text,
                    _4m = _4M
                };

                using var db = await _dbFactory.CreateDbContextAsync();
                db.TbScanOuts.Add(scanOutData);
                db.SaveChanges();

                // Cần cảnh báo khác EBR/WO

            }
            else if (data.Result.Equals("NG", StringComparison.Ordinal))
            {
                lbStatus.ForeColor = Color.Red;
            }
            else
            {
                lbStatus.ForeColor = Color.Yellow;
            }


        }

        public static string GetLast11(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Length <= 11 ? input : input.Substring(input.Length - 11);
        }

        public static (int CompletedPercent, int CompletedQty, int WoQty) ExtractProgress(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (0, 0, 0);

            // Regex with optional spaces: \s*
            var match = Regex.Match(input, @"(\d+)\s*%\s*\(\s*(\d+)\s*/\s*(\d+)\s*\)");

            if (!match.Success)
                return (0, 0, 0);

            return (
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value)
            );
        }
        private void SafeSetText(Control c, string text)
        {
            if (c.IsDisposed) return;
            if (c.InvokeRequired) c.BeginInvoke(new Action(() => c.Text = text));
            else c.Text = text;
        }
        private void SafeSetText(ToolStripStatusLabel lbl, string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => lbl.Text = text));
            }
            else
            {
                lbl.Text = text;
            }
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            using var font = new Font("Arial", 10);
            using var bold = new Font("Arial", 12, FontStyle.Bold);

            int left = 40, top = 40;
            int col1 = 140, col2 = 260;
            int rowH = 30, headerH = 34;
            int rowsCount = 8;   // ✅ was 7, now 8 (extra barcode row)
            int totalH = headerH + rowsCount * rowH;

            // border
            g.DrawRectangle(Pens.Black, left, top, col1 + col2, totalH);

            // header
            var headerRect = new Rectangle(left, top, col1 + col2, headerH);
            using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("Manufacturing Tag", bold, Brushes.Black, headerRect, sfCenter);

            int y = top + headerH;
            g.DrawLine(Pens.Black, left, y, left + col1 + col2, y);

            string[,] rows =
            {
                { "Model",         tagLabel.Model },
                { "Date",          tagLabel.Date },
                { "Part No",       tagLabel.PartNo },
                { "Q’TY",          tagLabel.Quantity.ToString() },
                { "1st Inspector", tagLabel.Inspector1 },
                { "2nd Inspector", tagLabel.Inspector2 },
                { "4M",            tagLabel.Process },
                { "Barcode",       "" } // ✅ empty, we’ll draw image instead
            };

            for (int i = 0; i < rowsCount; i++)
            {
                var rowTop = y + i * rowH;

                // vertical split
                g.DrawLine(Pens.Black, left + col1, rowTop, left + col1, rowTop + rowH);
                // bottom line
                g.DrawLine(Pens.Black, left, rowTop + rowH, left + col1 + col2, rowTop + rowH);

                // left column label
                g.DrawString(rows[i, 0], font, Brushes.Black, new RectangleF(left + 6, rowTop + 6, col1 - 12, rowH - 12));

                if (i < rowsCount - 1) // normal rows
                {
                    g.DrawString(rows[i, 1], font, Brushes.Black,
                        new RectangleF(left + col1 + 6, rowTop + 6, col2 - 12, rowH - 12));
                }
                else // ✅ barcode row
                {
                    var barcodeImg = GenerateBarcode(tagLabel.TagId);
                    g.DrawImage(barcodeImg, left + col1 + 6, rowTop + 2, col2 - 12, rowH - 4);
                }
            }


        }

        private Bitmap GenerateBarcode(long value)
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 40,
                    Width = 120,
                    Margin = 1,
                    PureBarcode = false
                },
                Renderer = new BitmapRenderer()   // <- comes from ZXing.Rendering
            };

            return writer.Write(value.ToString());
        }

        private void SetupPortScanner()
        {
            var section = _config.GetSection("PortScanner");

            string portName = section["PortName"] ?? "COM1";
            int baudRate = int.Parse(section["BaudRate"] ?? "9600");
            Parity parity = Enum.Parse<Parity>(section["Parity"] ?? "None");
            int dataBits = int.Parse(section["DataBits"] ?? "8");
            StopBits stopBits = Enum.Parse<StopBits>(section["StopBits"] ?? "One");

            _portScanner = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _portScanner.DataReceived += PortScanner_DataReceived;
            _portScanner.Open();
        }
        private void SetupPortSFC()
        {
            var section = _config.GetSection("PortSFC");

            string portName = section["PortName"] ?? "COM1";
            int baudRate = int.Parse(section["BaudRate"] ?? "9600");
            Parity parity = Enum.Parse<Parity>(section["Parity"] ?? "None");
            int dataBits = int.Parse(section["DataBits"] ?? "8");
            StopBits stopBits = Enum.Parse<StopBits>(section["StopBits"] ?? "One");

            _portSFC = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _portSFC.DataReceived += PortSFC_DataReceived;
            _portSFC.Open();
        }
        private async void PortScanner_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            CurrentPid = _portScanner.ReadExisting();

            using var db = await _dbFactory.CreateDbContextAsync();

            // 1. Check PID is block?
            var isBlock = await db.TbBlocks
                .Where(x => x.Pid.ToLower() == CurrentPid.ToLower() && x.Status.ToLower() == "b")
                .FirstOrDefaultAsync();
            if (isBlock != null)
            {
                ShowDetailStatus($"{CurrentPid} {isBlock.History}");
                return;
            }

            // 2. Send PID to SFC
            _portSFC.WriteLine($"{CurrentPid}");

            //// 3. tb_tag_temp
            //tagData.PidList.Add(data);
            //tagData = new ManufacturingTagDto()
            //{
            //    Model = "Honda 25.5MY PCBA",
            //    Date = DateTime.Now.ToString("d/M/yyyy"),
            //    PartNo = "xxx",   // scanned value
            //    Quantity = 1,
            //    Inspector1 = "Jose",
            //    Inspector2 = "Diego",
            //    FourM = "4M",
            //    Process = "AUTOMATIC FCT",
            //    TagId = "XXXXXXXXXXXXX"
            //};

        }
        private void PortSFC_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //string data = _portSFC.ReadExisting();
            ////BeginInvoke(new Action(() => textBox1.AppendText(data)));
        }
        void ShowDetailStatus(string text)
        {
            BeginInvoke(new Action(() =>
                  lbDetailStatus.Text = $"⚠️ {text}"
              ));
        }
        void SetupDonut(Chart chart)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Annotations.Clear();
            chart.Palette = ChartColorPalette.None;

            var ca = new ChartArea("ca") { BackColor = Color.White };
            chart.ChartAreas.Add(ca);

            var s = new Series("wo")
            {
                ChartType = SeriesChartType.Doughnut,
                ChartArea = "ca",
                IsVisibleInLegend = false,
                IsValueShownAsLabel = false,      // no slice labels
                BorderWidth = 0
            };
            s["DoughnutRadius"] = "40";
            s["PieStartAngle"] = "270";
            chart.Series.Add(s);
        }

        // call whenever values change
        void SetWorkOrderProgress(Chart chart, System.Windows.Forms.Label lblSummary, int completed, int total)
        {
            var s = chart.Series["wo"];
            s.Points.Clear();

            // Ensure annotation exists
            var ann = chart.Annotations.FindByName("centerPct") as TextAnnotation;
            if (ann == null)
            {
                ann = new TextAnnotation
                {
                    Name = "centerPct",
                    IsSizeAlwaysRelative = true,
                    ClipToChartArea = "ca", // <- make sure this matches your ChartArea name
                    AnchorAlignment = ContentAlignment.MiddleCenter,
                    LineColor = Color.Transparent
                };
                chart.Annotations.Add(ann);
            }
            ann.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            ann.ForeColor = Color.Black;
            ann.AnchorX = 50;
            ann.AnchorY = 50; // center of chart area (%)

            // === Special handling: total == 0 => ignore calc, show dashes, 0% ring ===
            if (total <= 0)
            {
                // Render as 0%: completed=0, remain=1 (to draw a full gray ring)
                int iDone = s.Points.AddY(0);
                int iRemain = s.Points.AddY(1);
                s.Points[iDone].Color = Color.FromArgb(42, 184, 75); // green
                s.Points[iRemain].Color = Color.Gainsboro;           // gray

                ann.Text = "-";
                if (lblSummary != null) lblSummary.Text = "- / - (-)";
                return;
            }

            // === Normal path ===
            total = Math.Max(1, total);
            completed = Math.Max(0, Math.Min(completed, total));
            int remain = total - completed;
            double pct = (double)completed / total;

            int iDoneN = s.Points.AddY(completed);
            int iRemainN = s.Points.AddY(remain);
            s.Points[iDoneN].Color = Color.FromArgb(42, 184, 75); // green
            s.Points[iRemainN].Color = Color.Gainsboro;           // gray

            ann.Text = $"{pct:P0}"; // e.g., 88%

            if (lblSummary != null)
                lblSummary.Text = $"{completed} / {total} ({remain})";
        }

        private async void btnPrint_Click(object sender, EventArgs e)
        {
            // 1. Get tag data to print
            using var db = await _dbFactory.CreateDbContextAsync();
            var tagDatas = await db.TbTagTemps
                .Where(x => x.Pid.ToLower() == CurrentPid.ToLower() && x.Client == ip && x.PartNo == "Current P/No")
                .ToListAsync();
            if (tagDatas == null)
            {
                ShowDetailStatus($"Not found any data matching for {CurrentPid}");
                return;
            }
            DateTime now = DateTime.Now;
            long tagIdValue = long.Parse(now.ToString("yyMMddHHmmssffff"));

            tagLabel = new ManufacturingTagDto()
            {
                Model = tagDatas[0].Model,
                Date = now.ToString("dd/MM/yyyy"),
                PartNo = tagDatas[0].PartNo,
                Quantity = tagDatas.Count,
                Inspector1 = "",
                Inspector2 = "",
                FourM = "",
                Process = "",
                TagId = tagIdValue,
                PidList = tagDatas.Select(x => x.Pid).ToList()
            };

            // 2. Print
            PrintDialog dlg = new PrintDialog();
            dlg.Document = printDoc;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                printDoc.PrinterSettings = dlg.PrinterSettings;
                printDoc.Print();
                //tagData = new ManufacturingTagDto();
            }

            // 3. Update tagID for matched PID in tb_scan_out
            await db.TbScanOuts
            .Where(x => tagLabel.PidList.Contains(x.Pid))
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.TagId, r => tagIdValue)
                .SetProperty(r => r.PrintAt, r => DateTime.Now)
            );
            // 4. Delete tag data at tb_tab_temp
            await db.TbTagTemps
                .Where(x => tagLabel.PidList.Contains(x.Pid))
                .ExecuteDeleteAsync();
            // 5. Reset tagLabel & CurrentPid
            tagLabel = new ManufacturingTagDto();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                var x = _ui.ClickClear();
            }
            catch
            {

                MessageBox.Show("No se puede enviar la operación, abra el software SFC.LGE");
            }

        }

        private void fpnScanData_ControlAdded(object sender, ControlEventArgs e)
        {
            HighlightFlash(e.Control, Color.Khaki, 5000); // 800 ms
        }
        void HighlightFlash(Control c, Color flashColor, int durationMs = 800)
        {
            var orig = c.BackColor;
            c.BackColor = flashColor;

            var t = new System.Windows.Forms.Timer { Interval = durationMs };
            t.Tick += (_, __) => { t.Stop(); t.Dispose(); c.BackColor = orig; };
            t.Start();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cbtnConfirmSetting_CheckedChanged(object sender, EventArgs e)
        {
            if (cbtnConfirmSetting.Checked)
            {
                txt1stInspetor.Enabled = false;
                txt2ndInspector.Enabled = false;
                txtSettingEBR.Enabled = false;
            }
            else
            {
                txt1stInspetor.Enabled = true;
                txt2ndInspector.Enabled = true;
                txtSettingEBR.Enabled = true;
            }
        }
    }
}
