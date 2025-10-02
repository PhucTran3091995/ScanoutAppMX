using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;   // for TreeScope, ControlType, etc.
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Patterns;             // ValuePattern (for ValueProperty)
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using FlaUI.UIA3.Patterns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MSFC.Data;
using MSFC.Dto;
using MSFC.Models;
using MSFC.Service;
using MSFC.Service.Translate;
using MSFC.UC;
using QRCoder;
using ScanOutTool.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Media;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices; // COMException
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms.DataVisualization.Charting;
using UIAutoLib.Interfaces;
using UIAutoLib.Services;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using static FlaUI.Core.FrameworkAutomationElementBase;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        private string hostName = Dns.GetHostName();
        private string _ClientIp;


        //========== UI automation theo GMES 2.0 ==========
        private IAutomationService2 _automationService2;
        private readonly AutomationConfig2 _config2;
        private IReadOnlyList<AutomationElement> _cachedNodes;
        private Dictionary<string, AutomationElement> _controlCache;
        string CurrentPid;
        private CancellationTokenSource _pidCts;
        private Task _pidTask;
        private volatile bool _building;      // chống chồng lấp
        private readonly object _pidLock = new();
        private string? _lastPidValue;
        private string? _lastStatusValue;
        //========== UI automation theo GMES 2.0 ==========
        private readonly ITranslatorService _translator;

        //========== Sound alarm ==========
        SoundPlayer OKplayer = new SoundPlayer(new MemoryStream(Properties.Resources.PASS));
        SoundPlayer NGplayer = new SoundPlayer(new MemoryStream(Properties.Resources.FAIL));

        //========== Data for tab label ==========
        private ManufacturingTagDto _tagLabel = new ManufacturingTagDto();
        string _inspector1 = string.Empty;
        string _inspector2 = string.Empty;
        string _4M = string.Empty;
        string _settingEBR = string.Empty;
        //========== Quản lý PID đã scan tại thời điểm hiện tại để phục vụ in tem ==========
        private readonly object _pendingLock = new();
        private readonly HashSet<string> _pendingPids = new(StringComparer.OrdinalIgnoreCase);

        public Form1(
            IDbContextFactory<MMesDbContext> dbFactory,
            IConfiguration config,
            IAutomationService2 automationService2,
            IOptions<AutomationConfig2> config2,
            ITranslatorService translator)
        {
            _dbFactory = dbFactory;
            _config = config;
            var section = _config.GetSection("TagSetting");
            _4M = section["4M"] ?? string.Empty;

            InitializeComponent();

            // Setup Chart
            SetupDonut(chartProgress);

            //// Setup COM
            //SetupPortScanner();
            //SetupPortSFC();


            // Printer
            //printDoc.PrintPage += GenerateLabel;

            // Get IP of client
            _ClientIp = Dns.GetHostAddresses(hostName)
                       .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                       ?.ToString() ?? DateTime.Now.ToString("ffff");


            //========== UI automation theo GMES 2.0 ==========
            _automationService2 = automationService2;
            _config2 = config2.Value;

            //========== Translation ==========
            _translator = translator;
            ////========== Sound ==========
            //OKplayer.Load();
            //NGplayer.Load();
        }

        private void ResetRtxtDetailExplain()
        {
            rtxtDetailExplain.Clear();
            rtxtDetailExplain.ForeColor = Color.Black;
            rtxtDetailExplain.BackColor = SystemColors.Window;
            rtxtDetailExplain.Font = new Font(rtxtDetailExplain.Font.FontFamily, 9, FontStyle.Regular);
        }
        public bool IsWarningEbrMessage(string msg)
        {
            var pattern = @"^⚠️ P/No escaneado \(.*?\) no coincide con P/No configurado \(.*?\)\.$";
            return Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase);
        }
        private void ShowNoticeAndExplain(string msg)
        {
            //int line = rtxtDetailExplain.Lines.Length + 1;

            //if (!string.IsNullOrEmpty(msg))
            //{
            //    if (!IsWarningEbrMessage(msg)) // Nếu không phải cảnh báo sai EBR thì cần tra dict để hiển thị hướng dẫn + thông báo bằng Es
            //    {
            //        var res = _translator.TranslateAndGuide(msg);
            //        lbDetailStatus.Text = _translator.Translate(res.Translated);
            //        rtxtDetailExplain.AppendText($"\u2605 {res.ExplainEs}\r\n");
            //    }
            //    else
            //    {
            //        rtxtDetailExplain.AppendText($"\u2605 {msg} → Por favor clasificar en el lugar correcto.\r\n");
            //    }
            //}




        }

        private void UpdateUI(PCB data)
        {
            if (data == null) return;
            if (!this.IsHandleCreated || this.IsDisposed) return;

            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    ResetRtxtDetailExplain();

                    // 1) Chuẩn hoá chuỗi để so sánh an toàn
                    var resultRaw = data.Result ?? string.Empty;
                    var resultClean = resultRaw.Trim();             // hoặc CleanResult(resultRaw)
                    var ebrCur = (data.EBR ?? string.Empty).Trim();
                    var ebrSetting = (_settingEBR ?? string.Empty).Trim();

                    // 2) Phân loại kết quả gốc
                    var kind = Classify(resultClean);               // Ok / Ng / Ready

                    // 3) Nếu OK nhưng EBR khác setting -> ép lỗi (mismatch coi như NG)
                    bool hasSetting = !string.IsNullOrWhiteSpace(ebrSetting);
                    bool mismatch = hasSetting && !string.Equals(ebrCur, ebrSetting, StringComparison.OrdinalIgnoreCase);

                    var finalKind = kind;
                    if (kind == ResultKind.Ok && mismatch)
                    {
                        finalKind = ResultKind.Ng;                  // ép NG khi lệch EBR
                    }

                    // 4) Áp style theo finalKind (không theo kind gốc)
                    ApplyStatusStyle(finalKind);

                    // 5) Gắn EBR setting lần đầu nếu trống và kết quả OK (không có setting để so sánh)
                    if (kind == ResultKind.Ok && string.IsNullOrWhiteSpace(txtSettingEBR.Text))
                    {
                        txtSettingEBR.Text = ebrCur;
                        _settingEBR = ebrCur;
                        // cập nhật lại ebrSetting/hasSetting nếu muốn, nhưng không cần ngay bây giờ
                    }

                    // 6) Nếu có PID thì render chi tiết
                    if (!string.IsNullOrWhiteSpace(data.PID))
                    {
                        // 6.1) Cảnh báo mismatch (nếu có)
                        if (mismatch)
                        {
                            var msg = $"⚠️ P/No escaneado ({ebrCur}) no coincide con P/No configurado ({ebrSetting}).";
                            ShowNoticeAndExplain(msg);
                        }

                        // 6.2) Âm thanh theo finalKind (KHÔNG phát OK khi lệch EBR)
                        if (finalKind == ResultKind.Ng) _ = PlayNgAsync();
                        else if (finalKind == ResultKind.Ok) _ = PlayOkAsync();

                        // 6.3) Left panel
                        lbModelSuffixData.Text = ebrCur;
                        lbWoData.Text = data.WO ?? string.Empty;
                        lbBuyerData.Text = data.Buyer ?? string.Empty;

                        // 6.4) Right panel
                        lbPID.Text = $"PCBA S/N: {data.PID}";
                        lbStatus.Text = mismatch ? "OK (Desajuste de EBR)" : resultRaw; // thể hiện rõ “OK có lệch EBR”

                        // 6.5) Message detail (nếu có)
                        if (!string.IsNullOrWhiteSpace(data.Message))
                            ShowNoticeAndExplain(data.Message);

                        // 6.6) Progress: CHỈ cập nhật khi finalKind là OK (tức không mismatch)
                        if (finalKind == ResultKind.Ok)
                        {
                            var (completedQty, woQty) = ExtractProgressSafe(data.Progress);
                            var total = woQty;
                            var completed = completedQty;
                            var remain = Math.Max(0, total - completed);

                            lbRemainQtyData.Text = remain.ToString();
                            SetWorkOrderProgress(chartProgress, lbProgressData,
                                                 completed: Math.Max(0, completed),
                                                 total: Math.Max(1, total)); // tránh /0
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddLog("UI error: " + ex.Message);
                }
            }));
        }


        /// <summary>
        /// ExtractProgress an toàn: không ném exception, trả (0,0) khi input không khớp.
        /// Kỳ vọng: "161 / 360" hoặc "45% (161/360)" → tuỳ regex bạn đang hỗ trợ.
        /// </summary>
        private static (int CompletedQty, int WoQty) ExtractProgressSafe(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    return (0, 0);

                // 1) Ưu tiên pattern có %, ví dụ: "45% (161/360)"
                var m1 = System.Text.RegularExpressions.Regex.Match(input, @"(\d+)\s*%\s*\(\s*(\d+)\s*/\s*(\d+)\s*\)");
                if (m1.Success)
                {
                    int completed = int.Parse(m1.Groups[2].Value);
                    int total = int.Parse(m1.Groups[3].Value);
                    return (completed, total);
                }

                // 2) Fallback pattern đơn giản: "161 / 360"
                var m2 = System.Text.RegularExpressions.Regex.Match(input, @"(\d+)\s*/\s*(\d+)");
                if (m2.Success)
                {
                    int completed = int.Parse(m2.Groups[1].Value);
                    int total = int.Parse(m2.Groups[2].Value);
                    return (completed, total);
                }

                return (0, 0);
            }
            catch
            {
                return (0, 0);
            }
        }

        private void resetUI()
        {
            try
            {


                // Clear trên app này
                UpdateUI(new PCB
                {
                    PID = "",
                    EBR = "",
                    WO = "",
                    Result = "",
                    Message = "",
                    Progress = "0/0"
                });

                // guide
                ResetRtxtDetailExplain();
                // status
                lbStatus.Text = "LISTA";
                lbStatus.ForeColor = Color.Yellow;
                lbStatus.BackColor = Color.Black;
                // detail sattus
                lbDetailStatus.Text = "-";
                lbDetailStatus.ForeColor = Color.Black;
                // Information
                lbModelSuffixData.Text = "-";
                lbWoData.Text = "-";

                //Scan item
                lbPID.Text = "-";
                _lastPidValue = string.Empty;

                // Clear lỗi trên SFC
                _automationService2.ClickTo(_config2.Controls.ClearButton);

            }
            catch
            {

                MessageBox.Show("No se puede enviar la operación, abra el software SFC.LGE");
            }
        }

        private async Task PlayNgAsync()
        {
            NGplayer.PlaySync();
        }
        private async Task PlayOkAsync()
        {
            OKplayer.PlaySync();
        }
        private enum ResultKind { Ok, Ng, Other }

        private static bool HasWord(string s, string word)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(
                s, $@"\b{System.Text.RegularExpressions.Regex.Escape(word)}\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private static ResultKind Classify(string result)
        {
            if (HasWord(result, "OK")) return ResultKind.Ok;
            if (HasWord(result, "NG")) return ResultKind.Ng;
            return ResultKind.Other;
        }
        private void ApplyStatusStyle(ResultKind kind)
        {

            // Cho label trong suốt để nhìn màu panel
            lbStatus.BackColor = Color.Transparent;

            switch (kind)
            {
                case ResultKind.Ok:
                    //pnStatus.BackColor = Color.Lime;
                    lbStatus.ForeColor = Color.Black;
                    lbStatus.BackColor = Color.Lime;
                    lbDetailStatus.Text = "";
                    rtxtDetailExplain.Text = "";

                    break;
                case ResultKind.Ng:
                    //pnStatus.BackColor = Color.Red;
                    lbStatus.ForeColor = Color.Black;
                    lbStatus.BackColor = Color.Red;

                    rtxtDetailExplain.ForeColor = Color.Red;
                    rtxtDetailExplain.BackColor = Color.Yellow;
                    rtxtDetailExplain.Font = new Font(rtxtDetailExplain.Font.FontFamily, 26, FontStyle.Bold);

                    lbDetailStatus.ForeColor = Color.Yellow;
                    break;
                default:
                    pnStatus.BackColor = Color.Black;
                    lbStatus.ForeColor = Color.Yellow;
                    lbStatus.BackColor = Color.Black;
                    break;
            }
        }

        private async Task UploadToDb(PCB data, CancellationToken ct = default)
        {
            //// B1: đọc control UI trên UI thread
            //var ui = await OnUiAsync(() => new
            //{
            //    Inspector1 = txtInspector1.Text?.Trim(),
            //    Inspector2 = txtInspector2.Text?.Trim()
            //});

            //// B2: đọc config (không cần UI thread)
            //var section = _config.GetSection("TagSetting");
            //string _4M = section["4M"] ?? string.Empty;


            // Chuẩn bị entity
            var scanOutData = new TbScanOut
            {
                ClientId = _ClientIp,
                Pid = data?.PID,
                ModelSuffix = data?.EBR,
                PartNo = data?.EBR,
                WorkOrder = data?.WO,
                ScanAt = DateTime.Now,
                Qty = 1,
                FirstInspector = _inspector1,
                SecondInspector = _inspector2,
                _4m = _4M,
                ScanDate = DateOnly.FromDateTime(DateTime.Now)

            };

            // B3: DB I/O (không UI thread)
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

                // Check tồn tại theo PID
                bool exists = await db.TbScanOuts
                    .AnyAsync(x => x.Pid == data.PID, ct)
                    .ConfigureAwait(false);

                if (!exists)
                {
                    await db.TbScanOuts.AddAsync(scanOutData, ct).ConfigureAwait(false);
                    await db.SaveChangesAsync(ct).ConfigureAwait(false);
                    lock (_pendingLock)
                    {
                        if (!string.IsNullOrWhiteSpace(data.PID))
                            _pendingPids.Add(data.PID.Trim());
                    }
                }

                // Giả sử data.PID là khóa duy nhất


            }
            catch (Exception ex)
            {
                // log hoặc hiện thông báo (UI thông báo nên đưa qua OnUiAsync)
                AddLog("DB error: " + ex.Message);
                // tùy nhu cầu: rethrow hoặc nuốt
                // throw;
            }
        }


        public static string GetLast11(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Length <= 11 ? input : input.Substring(input.Length - 11);
        }

        public static (int CompletedQty, int WoQty) ExtractProgress(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (0, 0);

            input = input.Trim();

            // Dạng 1: "45% (161/360)" (cho phép khoảng trắng tự do)
            var m1 = Regex.Match(input, @"^\s*(\d+)\s*%\s*\(\s*([\d,\.]+)\s*/\s*([\d,\.]+)\s*\)\s*$");
            if (m1.Success)
                return (ParseInt(m1.Groups[2].Value), ParseInt(m1.Groups[3].Value));

            // Dạng 2: "161 / 360" hoặc "161/360"
            var m2 = Regex.Match(input, @"^\s*([\d,\.]+)\s*/\s*([\d,\.]+)\s*$");
            if (m2.Success)
                return (ParseInt(m2.Groups[1].Value), ParseInt(m2.Groups[2].Value));

            // (Tuỳ chọn) Dạng 3: "161 of 360" / "161 de 360"
            var m3 = Regex.Match(input, @"^\s*([\d,\.]+)\s*(?:of|de)\s*([\d,\.]+)\s*$", RegexOptions.IgnoreCase);
            if (m3.Success)
                return (ParseInt(m3.Groups[1].Value), ParseInt(m3.Groups[2].Value));

            return (0, 0);
        }

        private static int ParseInt(string s)
        {
            // Bỏ dấu phân tách nghìn phổ biến rồi parse
            s = (s ?? string.Empty).Replace(",", "").Replace(".", "");
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;
        }
        //private void GenerateLabel(object sender, PrintPageEventArgs e)
        //{
        //    var g = e.Graphics;
        //    using var font = new Font("Arial", 10);
        //    using var bold = new Font("Arial", 12, FontStyle.Bold);

        //    int left = 40, top = 40;
        //    int col1 = 140, col2 = 260;
        //    int rowH = 30, headerH = 34;
        //    int rowsCount = 8;   // ✅ was 7, now 8 (extra barcode row)
        //    int totalH = headerH + rowsCount * rowH;

        //    // border
        //    g.DrawRectangle(Pens.Black, left, top, col1 + col2, totalH);

        //    // header
        //    var headerRect = new Rectangle(left, top, col1 + col2, headerH);
        //    using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        //    g.DrawString("Manufacturing Tag", bold, Brushes.Black, headerRect, sfCenter);

        //    int y = top + headerH;
        //    g.DrawLine(Pens.Black, left, y, left + col1 + col2, y);

        //    string[,] rows =
        //    {
        //        { "Model",         tagLabel.Model },
        //        { "Date",          tagLabel.Date },
        //        { "Part No",       tagLabel.PartNo },
        //        { "Q’TY",          tagLabel.Quantity.ToString() },
        //        { "1st Inspector", tagLabel.Inspector1 },
        //        { "2nd Inspector", tagLabel.Inspector2 },
        //        { "4M",            tagLabel.Process },
        //        { "Barcode",       "" }
        //    };

        //    for (int i = 0; i < rowsCount; i++)
        //    {
        //        var rowTop = y + i * rowH;

        //        // vertical split
        //        g.DrawLine(Pens.Black, left + col1, rowTop, left + col1, rowTop + rowH);
        //        // bottom line
        //        g.DrawLine(Pens.Black, left, rowTop + rowH, left + col1 + col2, rowTop + rowH);

        //        // left column label
        //        g.DrawString(rows[i, 0], font, Brushes.Black, new RectangleF(left + 6, rowTop + 6, col1 - 12, rowH - 12));

        //        if (i < rowsCount - 1) // normal rows
        //        {
        //            g.DrawString(rows[i, 1], font, Brushes.Black,
        //                new RectangleF(left + col1 + 6, rowTop + 6, col2 - 12, rowH - 12));
        //        }
        //        else // ✅ barcode row
        //        {
        //            var barcodeImg = GenerateBarcode(tagLabel.TagId);
        //            g.DrawImage(barcodeImg, left + col1 + 6, rowTop + 2, col2 - 12, rowH - 4);
        //        }
        //    }


        //}

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
        string IpToNumericString(string ip)
        {
            return string.Join("", ip.Split('.').Select(b => int.Parse(b).ToString("D3")));
        }

        long GenerateTagId()
        {
            DateTime now = DateTime.Now;

            string ipNum = IpToNumericString(_ClientIp);
            // timestamp yyMMddHHmmssffff
            string timestamp = now.ToString("yyMMddHHmmssfff");
            var last6Digits = ipNum.Substring(ipNum.Length - 3);
            // Ghép lại
            string tagIdStr = last6Digits + timestamp;
            long tagIdValue = long.Parse(tagIdStr);

            return tagIdValue;
        }



        //private async void btnPrint_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (lbStatus.Text.Equals("OK", StringComparison.OrdinalIgnoreCase))
        //        {
        //            AddLog($"Start query\r\n");
        //            // 1. Get tag data to print
        //            using var db = await _dbFactory.CreateDbContextAsync();
        //            var tagDatas = await db.TbScanOuts
        //                .Where(x =>
        //                x.ClientId == _ClientIp &&
        //                x.PartNo == _settingEBR &&
        //                !x.TagId.HasValue
        //                )
        //                .ToListAsync();

        //            if (tagDatas == null)
        //            {
        //                ShowDetailStatus($"Not found any data matching for {_lastPidValue}");
        //                return;
        //            }
        //            AddLog($"Gen TagID\r\n");
        //            DateTime now = DateTime.Now;
        //            long tagId = GenerateTagId();

        //            AddLog($"Assign data for tag label\r\n");
        //            _tagLabel = new ManufacturingTagDto()
        //            {
        //                Model = tagDatas[0].ModelSuffix,
        //                Date = now.ToString("dd/MM/yyyy"),
        //                PartNo = tagDatas[0].PartNo,
        //                Quantity = tagDatas.Count,
        //                Inspector1 = _inspector1,
        //                Inspector2 = _inspector2,
        //                FourM = _4M,
        //                Process = "",
        //                TagId = tagId,
        //                PidList = tagDatas.Select(x => x.Pid).ToList()
        //            };
        //            AddLog($"Print\r\n");
        //            PrintTag(_tagLabel);

        //            AddLog($"Udpate DB\r\n");
        //            // 3. Update tagID for matched PID in tb_scan_out
        //            await db.TbScanOuts
        //            .Where(x => _tagLabel.PidList.Contains(x.Pid))
        //            .ExecuteUpdateAsync(s => s
        //                .SetProperty(r => r.TagId, r => tagId)
        //                .SetProperty(r => r.PrintAt, r => DateTime.Now)
        //                .SetProperty(r => r.PrintDate, r => DateOnly.FromDateTime(DateTime.Now))
        //            );


        //            AddLog($"Reset global varible\r\n");
        //            // 5. Reset tagLabel & CurrentPid
        //            _tagLabel = new ManufacturingTagDto();
        //            _lastPidValue = string.Empty;
        //            _lastStatusValue = string.Empty;

        //            AddLog($"Reset UI\r\n");
        //            // 6. Reset UI & focus lại vào SFC
        //            resetUI();
        //        }
        //        else
        //        {
        //            var msg = $"El último PID escaneado debe ser el mismo que la configuración EBR y debe tener un resultado OK.";
        //            ShowDetailStatus(msg);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        AddLog(ex.Message);
        //    }


        //}
        private async void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (!lbStatus.Text.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    ShowDetailStatus("El último PID escaneado debe ser OK y coincidir con EBR.");
                    return;
                }

                // snapshot danh sách PID đang chờ in (batch hiện tại)
                string[] batchPids;
                lock (_pendingLock)
                {
                    batchPids = _pendingPids.ToArray();
                }

                if (batchPids.Length == 0)
                {
                    ShowDetailStatus("No hay PIDs pendientes para imprimir.");
                    return;
                }

                //AddLog("Start query (batch only)\r\n");

                using var db = await _dbFactory.CreateDbContextAsync();

                // Lấy đúng các dòng thuộc batch hiện tại, chưa in
                //var tagDatas = await db.TbScanOuts
                //    .Where(x => x.ClientId == _ClientIp
                //                && x.PartNo == _settingEBR
                //                && !x.TagId.HasValue
                //                && batchPids.Contains(x.Pid))
                //    .ToListAsync();
                var tagDatas = await (
                     from s in db.TbScanOuts
                     join m in db.TbModelDicts on s.PartNo equals m.PartNo
                     where s.ClientId == _ClientIp
                           && s.PartNo == _settingEBR
                           && !s.TagId.HasValue
                           && batchPids.Contains(s.Pid)
                     select new
                     {
                         PartNo = m.PartNo,
                         ModelName = m.ModelName,
                         Board = m.Board,
                         Pid = s.Pid
                     }
                ).ToListAsync();

                if (tagDatas.Count == 0)
                {
                    ShowDetailStatus("No se encontraron PIDs válidos en el lote actual.");
                    // loại các PID không còn match khỏi batch để tránh lặp vô hạn
                    lock (_pendingLock)
                    {
                        foreach (var pid in batchPids) _pendingPids.Remove(pid);
                    }
                    return;
                }

                //AddLog("Gen TagID\r\n");
                var now = DateTime.Now;
                long tagId = GenerateTagId();

                //AddLog("Assign data for tag label\r\n");
                _tagLabel = new ManufacturingTagDto
                {
                    ModelName = $"{tagDatas[0].ModelName}-{tagDatas[0].Board}",
                    Date = now.ToString("dd/MM/yyyy"),
                    PartNo = tagDatas[0].PartNo,
                    Quantity = tagDatas.Count,                // ✅ chỉ đếm batch hiện tại
                    TagId = tagId,
                    PidList = tagDatas.Select(x => x.Pid).ToList()
                };

                //AddLog("Print\r\n");
                PrintTag(_tagLabel);

                //AddLog("Update DB\r\n");
                await db.TbScanOuts
                    .Where(x => _tagLabel.PidList.Contains(x.Pid))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.TagId, r => tagId)
                        .SetProperty(r => r.PrintAt, r => DateTime.Now)
                        .SetProperty(r => r.PrintDate, r => DateOnly.FromDateTime(DateTime.Now))
                    );

                // Xóa đúng các PID vừa in khỏi batch
                lock (_pendingLock)
                {
                    foreach (var pid in _tagLabel.PidList)
                        _pendingPids.Remove(pid);
                }

                // Reset biến/UI
                _tagLabel = new ManufacturingTagDto();
                _lastPidValue = string.Empty;
                _lastStatusValue = string.Empty;
                resetUI();
            }
            catch (Exception ex)
            {
                AddLog(ex.Message);
            }
        }

        #region Print Setting & Generate label tag
        // offset cấu hình (mm). dương = dịch xuống / dịch sang phải
        float _offsetXmm = 0.0f;   // ví dụ 1.5f để dịch phải 1.5mm
        float _offsetYmm = 1.5f;   // ví dụ 1.5f để dịch xuống 1.5mm

        // đổi mm -> hundredths of an inch (đơn vị của MarginBounds)
        static int MmToHundredths(float mm) => (int)Math.Round(mm / 25.4f * 100f);

        void PrintTag(ManufacturingTagDto tag)
        {
            var doc = new PrintDocument();
            doc.PrinterSettings = new PrinterSettings();

            // lấy đúng size 50x25mm từ driver (197x98 hundredths-inch hoặc ngược)
            var ps = doc.PrinterSettings.PaperSizes
                .Cast<PaperSize>()
                .FirstOrDefault(p => Math.Abs(p.Width - 197) < 5 && Math.Abs(p.Height - 98) < 5
                                  || Math.Abs(p.Width - 98) < 5 && Math.Abs(p.Height - 197) < 5);
            if (ps != null) doc.DefaultPageSettings.PaperSize = ps;

            doc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
            doc.OriginAtMargins = true;              // (0,0) = MarginBounds.TopLeft
            doc.DefaultPageSettings.Landscape = false; // nếu bị xoay hãy thử true

            _tagLabel = tag;
            doc.PrintPage += GenerateLabel_FitMarginBounds;

            doc.Print();
        }
        void GenerateLabel_FitMarginBounds(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;

            // KHÔNG bù hard margins khi đã dùng OriginAtMargins + MarginBounds
            // g.TranslateTransform(-e.PageSettings.HardMarginX, -e.PageSettings.HardMarginY); // ❌ bỏ

            // Canvas thiết kế theo đơn vị 1/100 inch (giống PrintTagTest)
            float designW = 197f; // ~50mm
            float designH = 98f;  // ~25mm

            // Vùng in thật driver cho phép
            var dest = e.MarginBounds;
            // >>> ÁP OFFSET Ở ĐÂY <<<
            int offX = MmToHundredths(_offsetXmm);
            int offY = MmToHundredths(_offsetYmm);

            // dịch rect theo offset; clamp nhẹ cho an toàn
            dest = new Rectangle(
                dest.Left + offX,
                dest.Top + offY,
                dest.Width,
                dest.Height
            );
            // Ép full theo từng trục (không chừa trắng)
            float scaleX = dest.Width / designW;
            float scaleY = dest.Height / designH;

            // Gốc tại góc trên-trái của vùng in
            g.TranslateTransform(dest.Left, dest.Top);
            g.ScaleTransform(scaleX, scaleY);

            // Vẽ theo "design space" 0..designW x 0..designH
            DrawTag_DesignUnits(g, designW, designH, _tagLabel);

            e.HasMorePages = false;
        }
        /// <summary>
        /// Vẽ nội dung tem theo "design units" (1/100 inch), ví dụ W≈197, H≈98 cho tem 50x25mm.
        /// Không phụ thuộc DPI vì phần gọi đã scale-fit vào e.MarginBounds.
        /// </summary>
        void DrawTag_DesignUnits(Graphics g, float W, float H, ManufacturingTagDto tag)
        {
            // ———————————————————————————————————————————————————————————
            // 1) KHUNG & THAM SỐ BỐ CỤC CƠ BẢN
            // ———————————————————————————————————————————————————————————

            using var pen = new Pen(Color.Black, 1);
            //g.DrawRectangle(pen, 0, 0, W - 1, H - 1);   // khung ngoài mỏng

            // Giảm margin: cho hiển thị nhiều nội dung hơn
            float padX = W * 0.02f;   // ~2% bề rộng
            float padY = H * 0.06f;   // ~6% bề cao

            // Chia cột: 72% cho text, 28% cho QR (QR nhỏ lại)
            float leftW = W * 0.72f;
            float rightW = W - leftW;

            // Đường chia cột (chỉ để tách thị giác, có thể bỏ nếu muốn)
            g.DrawLine(pen, leftW, padY, leftW, H - padY);

            // Font chữ: Arial Narrow giúp chứa nhiều ký tự hơn trong bề ngang nhỏ
            using var fTitle = new Font("Segoe UI", 12, FontStyle.Bold, GraphicsUnit.Point);
            using var fBody = new Font("Arial Narrow", 9, FontStyle.Regular, GraphicsUnit.Point);
            using var fQtyStyle = new Font("Segoe UI", 12, FontStyle.Bold, GraphicsUnit.Point);

            // ———————————————————————————————————————————————————————————
            // 2) TIÊU ĐỀ
            // ———————————————————————————————————————————————————————————
            // Chiều cao tiêu đề ~20% tấm tem để còn chỗ cho text
            var titleRect = new RectangleF(padX, padY, leftW - padX * 2, H * 0.20f);
            var sfn = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            g.DrawString(FormatEbr(tag?.PartNo), fTitle, Brushes.Black, titleRect, sfn);

            // ———————————————————————————————————————————————————————————
            // 3) KHỐI TEXT 3 DÒNG (Model/Date/P-No) – dạng: "Label (Value)"
            //    Dùng DrawFitText để tự co cỡ chữ nếu quá dài, đảm bảo không bị cắt.
            // ———————————————————————————————————————————————————————————
            float yStart = titleRect.Bottom + H * 0.015f;  // một chút khoảng cách dưới tiêu đề
            float textH = (H - padY) - yStart;            // khoảng cao còn lại cho text
            float lineH = textH / 3f;                     // 3 dòng đều nhau
            var textW = leftW - padX * 2;               // bề ngang khối text

            // Ghép chuỗi theo yêu cầu:
            string line1 = $"{tag?.ModelName ?? string.Empty}";
            string line2 = $"Día {tag?.Date ?? string.Empty}";           // dạng DD/MM/YYYY
            string line3 = $"Cantidad {tag?.Quantity.ToString() ?? string.Empty}";

            // Vẽ từng dòng, tự co text để vừa khung
            DrawFitText(g, line1, fBody, Brushes.Black, new RectangleF(padX, yStart + 0 * lineH, textW, lineH), 6.0f);
            DrawFitText(g, line2, fBody, Brushes.Black, new RectangleF(padX, yStart + 1 * lineH, textW, lineH), 6.0f);
            DrawFitText(g, line3, fQtyStyle, Brushes.Black, new RectangleF(padX, yStart + 2 * lineH, textW, lineH), 6.0f);

            // ———————————————————————————————————————————————————————————
            // 4) QR CODE (NHỎ LẠI)
            // ———————————————————————————————————————————————————————————
            // Ô chứa QR nằm trong cột phải, chừa ít padding để QR nhỏ gọn hơn
            float rightPad = Math.Min(W * 0.01f, rightW * 0.05f); // padding size
            float qrBoxW = rightW - rightPad * 2;
            float qrBoxH = H - padY * 2;

            // Giữ ô vuông => side = min(w, h), và giảm bớt ~15% cho nhỏ hơn
            float side = Math.Min(qrBoxW, qrBoxH) * 1f; // qr code size 
            float qrX = leftW + (rightW - side) / 2f;
            float qrY = padY + (qrBoxH - side) / 2f;

            // Tạo QR: nhớ null-safe cho TagId
            using var qr = GenerateQRCode(tag?.TagId.ToString() ?? "-", 300); // hoặc (int)Math.Round(g.DpiX)
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor; // để pixel QR sắc nét
            g.DrawImage(qr, new RectangleF(qrX, qrY, side, side));
        }

        /// <summary>
        /// Vẽ text vào khung và TỰ GIẢM cỡ chữ (Point) để vừa chiều cao.
        /// Không wrap. Nếu vẫn không vừa ở cỡ tối thiểu -> vẽ với ellipsis.
        /// </summary>
        /// <param name="g">Graphics hiện tại</param>
        /// <param name="text">Chuỗi cần vẽ</param>
        /// <param name="baseFont">Font gốc (lấy họ chữ & style)</param>
        /// <param name="brush">Màu bút</param>
        /// <param name="rect">Khung cần vẽ</param>
        /// <param name="minPointSize">Cỡ chữ nhỏ nhất cho phép</param>
        static void DrawFitText(Graphics g, string text, Font baseFont, Brush brush, RectangleF rect, float minPointSize = 6.0f)
        {
            // Không xuống dòng; nếu dài quá sẽ xử lý bằng cách giảm cỡ chữ/ellipsis
            var fmt = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            // Thử giảm dần từ cỡ hiện tại đến min
            for (float sz = baseFont.Size; sz >= minPointSize; sz -= 0.5f)
            {
                using var f = new Font(baseFont.FontFamily, sz, baseFont.Style, GraphicsUnit.Point);
                // MeasureString với width giới hạn để ước lượng có vừa chiều cao không
                var measured = g.MeasureString(text, f, (int)Math.Ceiling(rect.Width), fmt);

                if (measured.Height <= rect.Height + 0.5f)
                {
                    g.DrawString(text, f, brush, rect, fmt);
                    return;
                }
            }

            // Fallback: cỡ min + ellipsis
            using var fMin = new Font(baseFont.FontFamily, minPointSize, baseFont.Style, GraphicsUnit.Point);
            g.DrawString(text, fMin, brush, rect, fmt);
        }

        /// <summary>
        /// Định dạng EBR để hiển thị: 
        /// "EBR12345678" → "EBR 1234 5678"
        /// Nếu không đúng pattern thì giữ nguyên.
        /// </summary>
        static string FormatEbr(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            input = input.Trim().ToUpperInvariant();

            // Nếu bắt đầu bằng EBR và có ít nhất 9 ký tự
            if (input.StartsWith("EBR") && input.Length > 7)
            {
                string prefix = input.Substring(0, 3); // "EBR"
                string rest = input.Substring(3);

                // Cắt thành block 4-4
                if (rest.Length >= 8)
                {
                    string part1 = rest.Substring(0, 4);
                    string part2 = rest.Substring(4, Math.Min(4, rest.Length - 4));
                    return $"{prefix} {part1} {part2}";
                }
                else if (rest.Length >= 4)
                {
                    string part1 = rest.Substring(0, 4);
                    string part2 = rest.Substring(4);
                    return $"{prefix} {part1} {part2}";
                }
            }

            return input;
        }


        private Bitmap GenerateQRCode(string value, int preferDpi = 300) //Qrcode
        {
            var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode(string.IsNullOrEmpty(value) ? "-" : value,
                                        QRCodeGenerator.ECCLevel.M);

            var qrCode = new QRCode(data);
            var bmp = qrCode.GetGraphic(8, Color.Black, Color.White, true);
            bmp.SetResolution(preferDpi, preferDpi);
            return bmp;
        }
        #endregion


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



        // Field guard để chống reentrancy & track watcher
        private bool _isSettingUp = false;
        private bool _isWatching = false;

        private async void cbtnConfirmSetting_CheckedChanged(object sender, EventArgs e)
        {

            if (_isSettingUp) return; // chống reentrancy
            _isSettingUp = true;

            try
            {
                if (cbtnConfirmSetting.Checked)
                {
                    // UI: trạng thái đang cài
                    lbDetailStatus.Text = "Instalando. Por favor espere....";
                    lbDetailStatus.ForeColor = Color.Yellow;

                    // tắt input
                    txtInspector1.Enabled = false;
                    txtInspector2.Enabled = false;
                    txtSettingEBR.Enabled = false;
                    cbtnConfirmSetting.Enabled = false; // chặn click lặp


                    try
                    {
                        _inspector1 = Normalize(txtInspector1.Text);
                        _inspector2 = Normalize(txtInspector2.Text);

                        // Kết nối DB (dispose đúng cách)
                        using var db = await _dbFactory.CreateDbContextAsync();
                        bool canConnect = await db.Database.CanConnectAsync();
                        var test = canConnect ? "DB conectado!" : "Falló la conexión DB!";

                        // Attach theo tên process (có thể ném exception -> rơi vào catch ngoài)
                        _automationService2.AttachToProcess(_config2.ProcessName);

                        await Task.Delay(300); // cho SFC ổn định 1 nhịp

                        // Nếu không connect được DB hoặc attach thất bại (đã ném exception) -> dừng tại đây
                        if (!canConnect)
                        {
                            lbDetailStatus.ForeColor = Color.Red;
                            lbDetailStatus.Text = "No se pudo conectar a la base de datos.";
                            // rollback trạng thái
                            cbtnConfirmSetting.Checked = false;
                            return;
                        }

                        lbDetailStatus.Text = $"Vinculado exitosamente con SFC (OK) | {test}";
                        lbDetailStatus.ForeColor = (lbDetailStatus.Text.Contains("(OK)") && canConnect)
                                                    ? Color.Lime
                                                    : Color.Yellow;
                        // Thao tác SFC & reset UI cục bộ
                        try
                        {
                            // Clear lỗi trên SFC
                            _automationService2.ClickTo(_config2.Controls.ClearButton);

                            ResetRtxtDetailExplain();
                            lbPID.Text = "-";

                        }
                        catch (Exception)
                        {
                            MessageBox.Show("No se puede enviar la operación, abra el software SFC.LGE");
                            // Không khởi động watcher nếu clear thất bại
                            cbtnConfirmSetting.Checked = false;
                            return;
                        }

                        // Khởi động watcher nếu chưa chạy
                        if (!_isWatching)
                        {
                            StartWatchingPidPollingTask(); // nếu có bản async -> await StartWatchingPidPollingTask();
                            _isWatching = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        lbDetailStatus.ForeColor = Color.Red;
                        lbDetailStatus.Text = $"Enlace directo al SFC: {ex.Message}";
                        // rollback trạng thái khi lỗi
                        cbtnConfirmSetting.Checked = false;
                        return;
                    }
                    finally
                    {
                        cbtnConfirmSetting.Enabled = true;
                    }
                }
                else
                {
                    txtSettingEBR.Clear();
                    // Tắt watcher trước khi mở lại input
                    try
                    {
                        if (_isWatching)
                        {
                            await StopWatchingPidAsync();
                            _isWatching = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                    finally
                    {
                        // mở input
                        txtInspector1.Enabled = true;
                        txtInspector2.Enabled = true;
                        txtSettingEBR.Enabled = true;
                    }
                }
            }
            finally
            {
                _isSettingUp = false;
            }
        }




        #region Thao tác với UI Automation của GMES 2.0
        private void btnClear_Click(object sender, EventArgs e)
        {


        }
        private void AddLog(string msg)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() =>
                {
                    rtxtDetailExplain.AppendText($"{msg}\r\n");
                }));
            }
        }



        public async Task StopWatchingPidAsync()
        {
            CancellationTokenSource? cts;
            Task? task;
            lock (_pidLock)
            {
                cts = _pidCts;
                task = _pidTask;
                _pidCts = null;
                _pidTask = null;
            }

            if (cts != null)
            {
                try { cts.Cancel(); }
                catch { /* ignore */ }
                finally { cts.Dispose(); }
            }

            if (task != null)
            {
                try { await task.ConfigureAwait(false); }
                catch (OperationCanceledException) { }
                catch (Exception ex) { AddLog("StopWatching error: " + ex.Message); }
            }
        }

        private async Task StartWatchingPidPollingTask()
        {
            // chống khởi động trùng + dọn CTS cũ (nếu có)
            lock (_pidLock)
            {
                if (_pidTask != null && !_pidTask.IsCompleted) return;

                _pidCts?.Dispose();
                _pidCts = new CancellationTokenSource();
                _pidTask = LoopAsync(_pidCts.Token);
            }

            await Task.CompletedTask;
        }

        //private async Task LoopAsync(CancellationToken ct)
        //{
        //    // nhịp thăm dò tối thiểu
        //    const int idleDelayMs = 100;
        //    const int busyDelayMs = 50;

        //    while (!ct.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            if (_building)
        //            {
        //                await Task.Delay(busyDelayMs, ct).ConfigureAwait(false);
        //                continue;
        //            }

        //            // --- ĐỌC PID trên UI thread ---
        //            string currentPid = await OnUiAsync(() =>
        //            {
        //                return Normalize(_automationService2.GetText(_config2.Controls.PID)?.Trim() ?? string.Empty);
        //            }).ConfigureAwait(false);

        //            string currentStatus = await OnUiAsync(() =>
        //            {
        //                return Normalize(_automationService2.GetText(_config2.Controls.ResultUc, _config2.Controls.ResultText) ?? string.Empty);
        //            }).ConfigureAwait(false);

        //            // PID trống -> thả chậm
        //            if (string.IsNullOrWhiteSpace(currentPid))
        //            {
        //                await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
        //                continue;
        //            }

        //            // Bỏ trùng PID & status (không xử lý lại)
        //            if (string.Equals(currentPid, _lastPidValue, StringComparison.OrdinalIgnoreCase) && string.Equals(currentStatus, _lastStatusValue, StringComparison.OrdinalIgnoreCase))
        //            {
        //                await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
        //                continue;
        //            }


        //            // BẮT ĐẦU xử lý PID MỚI
        //            _building = true;
        //            try
        //            {
        //                // Build dữ liệu (UI thread)
        //                var data = await OnUiAsync(() =>
        //                {
        //                    try
        //                    {
        //                        var ebr = _automationService2.GetText(_config2.Controls.ModelSuffixUc, "txtText");
        //                        var wo = _automationService2.GetText(_config2.Controls.WorkOrderUc, "txtText");
        //                        var result = _automationService2.GetText(_config2.Controls.ResultUc, _config2.Controls.ResultText);
        //                        var msg = _automationService2.GetInnerMessageFromResult(_config2.Controls.ResultUc, _config2.Controls.ResultText);
        //                        var prog = _automationService2.GetText(_config2.Controls.ResultQty);

        //                        return new PCB
        //                        {
        //                            PID = Normalize(currentPid),
        //                            EBR = Normalize(ebr),
        //                            WO = Normalize(wo),
        //                            Result = Normalize(result),
        //                            Message = Normalize(msg),
        //                            Progress = Normalize(prog)
        //                        };
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        AddLog("OnUi build error: " + ex.GetType().Name + " - " + ex.Message);
        //                        return null;
        //                    }
        //                }).ConfigureAwait(false);

        //                if (data == null)
        //                {
        //                    // KHÔNG cập nhật _lastPidValue để lần sau thử lại cùng PID
        //                    await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
        //                    continue;
        //                }

        //                // Cập nhật UI (UI thread)
        //                await OnUiAsync(() =>
        //                {
        //                    UpdateUI(data);
        //                    return 0;
        //                }).ConfigureAwait(false);

        //                // Chỉ sau khi UI cập nhật thành công mới xác nhận đã xử lý PID này
        //                _lastPidValue = currentPid;
        //                _lastStatusValue = currentStatus;

        //                // Phân loại & ghi DB (NỀN, không UI)
        //                var kind = Classify(data.Result); // Classify tự CleanResult bên trong (nếu có)
        //                if (kind == ResultKind.Ok)
        //                {
        //                    // snapshot trước khi ghi (không đụng UI)
        //                    var snap = new PCB
        //                    {
        //                        PID = data.PID,
        //                        EBR = data.EBR,
        //                        WO = data.WO,
        //                        Result = data.Result,
        //                        Message = data.Message,
        //                        Progress = data.Progress
        //                        // … thêm các trường cần thiết
        //                    };

        //                    // I/O async; KHÔNG bọc OnUiAsync
        //                    try
        //                    {
        //                        await UploadToDb(snap, ct).ConfigureAwait(false);
        //                    }
        //                    catch (OperationCanceledException) { }
        //                    catch (Exception ex)
        //                    {
        //                        AddLog("UploadToDb error: " + ex.Message);
        //                    }
        //                }
        //            }
        //            finally
        //            {
        //                _building = false;
        //            }
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            // thoát yên lặng
        //            break;
        //        }
        //        catch (Exception ex)
        //        {
        //            AddLog("Loop error: " + ex); // log toàn stack
        //            await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
        //        }
        //    }
        //}
        private async Task LoopAsync(CancellationToken ct)
        {

            var sw = Stopwatch.StartNew();
            const int idleDelayMs = 100;
            const int busyDelayMs = 50;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_building)
                    {
                        await Task.Delay(busyDelayMs, ct).ConfigureAwait(false);
                        continue;
                    }
                    AddLog($"1:{sw.ElapsedMilliseconds}ms");

                    // === 1) ĐỌC SNAPSHOT 1 LẦN TRÊN UI THREAD ===
                    _building = true;
                    var data = await OnUiAsync(() =>
                    {
                        try
                        {
                            string pid = Normalize(_automationService2.GetText(_config2.Controls.PID));
                            string result = Normalize(_automationService2.GetText(_config2.Controls.ResultUc, _config2.Controls.ResultText));
                            string ebr = Normalize(_automationService2.GetText(_config2.Controls.ModelSuffixUc, "txtText"));
                            string wo = Normalize(_automationService2.GetText(_config2.Controls.WorkOrderUc, "txtText"));
                            string msg = Normalize(_automationService2.GetInnerMessageFromResult(_config2.Controls.ResultUc, _config2.Controls.ResultText));
                            string prog = Normalize(_automationService2.GetText(_config2.Controls.ResultQty));

                            if (string.IsNullOrWhiteSpace(pid)) return null;

                            return new PCB
                            {
                                PID = pid,
                                Result = result,
                                EBR = ebr,
                                WO = wo,
                                Message = msg,
                                Progress = prog
                            };
                        }
                        catch (Exception ex)
                        {
                            AddLog("OnUi snapshot error: " + ex.Message);
                            return null;
                        }
                    }).ConfigureAwait(false);
                    AddLog($"2:{sw.ElapsedMilliseconds}ms");
                    if (data == null)
                    {
                        _building = false;
                        await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
                        continue;
                    }
                    AddLog($"3:{sw.ElapsedMilliseconds}ms");

                    // Dedup: nếu PID & Result (clean) trùng với lần trước → bỏ qua
                    var resClean = Normalize(data.Result);
                    if (string.Equals(data.PID, _lastPidValue, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(resClean, _lastStatusValue, StringComparison.OrdinalIgnoreCase))
                    {
                        _building = false;
                        await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
                        continue;
                    }
                    AddLog($"4:{sw.ElapsedMilliseconds}ms");
                    // === 2) DEBOUNCE/ỔN ĐỊNH TRẠNG THÁI RESULT ===
                    // Nếu chưa OK, chờ ngắn rồi đọc lại 1-2 lần để tránh miss khung READY→OK
                    if (Classify(resClean) != ResultKind.Ok)
                    {
                        var confirmed = await WaitStableOkAsync(maxTries: 3, delayMs: 80, ct);
                        if (confirmed != null)
                        {
                            // cập nhật data theo snapshot OK
                            data.Result = confirmed.Result;
                            data.Message = confirmed.Message;
                            data.Progress = confirmed.Progress;
                            resClean = Normalize(data.Result);
                        }
                    }
                    AddLog($"5:{sw.ElapsedMilliseconds}ms");
                    // === 3) CẬP NHẬT UI ===
                    await OnUiAsync(() =>
                    {
                        UpdateUI(data);
                        return 0;
                    }).ConfigureAwait(false);
                    AddLog($"6:{sw.ElapsedMilliseconds}ms");
                    // Cập nhật mốc dedup bằng GIÁ TRỊ ĐÃ CLEAN (nhất quán với Classify)
                    _lastPidValue = data.PID;
                    _lastStatusValue = resClean;

                    // === 4) UPLOAD DB KHI OK (sau khi đã debounce) ===
                    if (Classify(resClean) == ResultKind.Ok)
                    {
                        try
                        {
                            // snapshot tối thiểu cho DB
                            var snap = new PCB
                            {
                                PID = data.PID,
                                EBR = data.EBR,
                                WO = data.WO,
                                Result = data.Result,
                                Message = data.Message,
                                Progress = data.Progress
                            };
                            await UploadToDb(snap, ct).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            AddLog("UploadToDb error: " + ex.Message);
                        }
                    }

                    _building = false;
                    AddLog($"7:{sw.ElapsedMilliseconds}ms");
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    AddLog("Loop error: " + ex);
                    _building = false;
                    await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
                }
            }
        }
        private async Task<PCB?> WaitStableOkAsync(int maxTries, int delayMs, CancellationToken ct)
        {
            for (int i = 0; i < maxTries; i++)
            {
                await Task.Delay(delayMs, ct).ConfigureAwait(false);

                var probe = await OnUiAsync(() =>
                {
                    try
                    {
                        string result = Normalize(_automationService2.GetText(_config2.Controls.ResultUc, _config2.Controls.ResultText));
                        string msg = Normalize(_automationService2.GetInnerMessageFromResult(_config2.Controls.ResultUc, _config2.Controls.ResultText));
                        string prog = Normalize(_automationService2.GetText(_config2.Controls.ResultQty));
                        return new PCB { Result = result, Message = msg, Progress = prog };
                    }
                    catch { return null; }
                }).ConfigureAwait(false);

                if (probe == null) continue;

                if (Classify(Normalize(probe.Result)) == ResultKind.Ok)
                    return probe; // xác nhận OK
            }
            return null; // không ổn định thành OK
        }

        string Normalize(string s)
        {
            // 1. Nếu null, rỗng, toàn khoảng trắng → trả về chuỗi rỗng luôn
            if (string.IsNullOrWhiteSpace(s))
                return "";

            // 2. Chuẩn hoá Unicode sang FormKC (Compatibility Composition)
            //    - Ghép lại các ký tự tách rời (O + ́ → Ó)
            //    - Biến các ký tự fullwidth/compatibility thành bản ASCII (ＯＫ → OK)
            var norm = s.Normalize(NormalizationForm.FormKC);

            // 3. Loại bỏ ký tự "khó chịu"
            var sb = new StringBuilder(norm.Length);
            foreach (var ch in norm)
            {
                if (char.IsControl(ch)) continue;     // bỏ ký tự điều khiển (CR, LF, NULL…)
                //if (char.IsPunctuation(ch)) continue; // bỏ dấu câu/ký tự đặc biệt (. , ! ? …)
                //if (char.IsSymbol(ch)) continue;      // bỏ ký tự dạng symbol (©, ±, ÷, …)
                //if (char.IsPunctuation(ch) && ch != '/') continue; // bỏ dấu câu trừ khi là '/'
                sb.Append(ch);                        // giữ lại chữ cái, số, khoảng trắng bình thường
            }

            // 4. Gom nhiều khoảng trắng liên tiếp thành 1 và Trim 2 đầu
            var cleaned = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();

            // 5. Đưa toàn bộ về in hoa invariant (không phụ thuộc locale)
            return cleaned.ToUpperInvariant();
        }

        string Inspect(string s) => s == null ? "(null)" : string.Join(", ", s.Select(c => $"U+{(int)c:X04}"));
        private Task<T> OnUiAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() =>
                {
                    try { var r = func(); tcs.SetResult(r); }
                    catch (Exception ex) { tcs.SetException(ex); }
                }));
            }
            else tcs.SetResult(default);
            return tcs.Task;
        }
        // Chạy 1 action trên UI thread và trả Task
        public Task OnUiAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            if (InvokeRequired)
                BeginInvoke(new Action(() => { try { action(); tcs.SetResult(null); } catch (Exception ex) { tcs.SetException(ex); } }));
            else
                try { action(); tcs.SetResult(null); } catch (Exception ex) { tcs.SetException(ex); }
            return tcs.Task;
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            await StopWatchingPidAsync();
            base.OnFormClosing(e);
        }


        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            var msg = _translator.Translate("Already Shipped");
            MessageBox.Show(msg);
        }

        private void btnClearNotice_Click(object sender, EventArgs e)
        {
            resetUI();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void lbStatus_Click(object sender, EventArgs e)
        {

        }

        private void btnTraceAndPrint_Click(object sender, EventArgs e)
        {
            var frm = new frmTraceAndPrint(_dbFactory);
            frm.ShowDialog();
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            bool canConnect = await db.Database.CanConnectAsync();
            var test = canConnect ? "DB conectado!" : "Falló la conexión DB!";
            if (!canConnect)
            {
                lbDetailStatus.ForeColor = Color.Red;
                lbDetailStatus.Text = "No se pudo conectar a la base de datos.";
                // rollback trạng thái
                cbtnConfirmSetting.Checked = false;
                return;
            }

            lbDetailStatus.Text = $"Vinculado exitosamente con SFC (OK) | {test}";
            lbDetailStatus.ForeColor = (lbDetailStatus.Text.Contains("(OK)") && canConnect)
                                        ? Color.Lime
                                        : Color.Yellow;
        }
    }
}
