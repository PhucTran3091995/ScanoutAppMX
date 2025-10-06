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
using Microsoft.Win32;
using MSFC.Data;
using MSFC.Dto;
using MSFC.Models;
using MSFC.Service;
using MSFC.Service.Translate;
using MSFC.UC;
using QRCoder;
using ScanOutTool.Services;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices; // COMException
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
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
using Label = System.Windows.Forms.Label;


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
        private readonly StaWorker _uiaWorker = new StaWorker();   // dùng cho mọi lệnh UIA: GetText/Find...
        private PCB _lastUi;                                       // nếu bạn dùng so sánh UI

        //========== UI automation theo GMES 2.0 ==========
        private readonly ITranslatorService _translator;

        // handles cache
        private AutomationElement _pidEl, _resultEl, _ebrEl, _woEl, _qtyEl, _resultUcEl;
        private readonly object _elLock = new object();


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

            //========== Khởi tạo service dùng để upload db ==========
            InitUploadService();

            SetAutoStart(true);

            cbtnConfirmSetting.Checked = true;
            rtxtDetailExplain.ReadOnly = true;
            richTextBox1.ReadOnly = true;


        }
        private void EnsureHandles()
        {
            // tránh race nếu STA worker và thread khác cùng vào
            lock (_elLock)
            {
                if (_pidEl == null) _pidEl = _automationService2.GetControlById(_config2.Controls.PID);
                if (_resultEl == null) _resultEl = _automationService2.GetChildControl(_config2.Controls.ResultUc, _config2.Controls.ResultText);
                if (_ebrEl == null) _ebrEl = _automationService2.GetChildControl(_config2.Controls.ModelSuffixUc, "txtText");
                if (_woEl == null) _woEl = _automationService2.GetChildControl(_config2.Controls.WorkOrderUc, "txtText");
                if (_qtyEl == null) _qtyEl = _automationService2.GetControlById(_config2.Controls.ResultQty);
                if (_resultUcEl == null) _resultUcEl = _automationService2.GetControlById(_config2.Controls.ResultUc);
            }
        }

        private void InvalidateHandlesIfNulls(params string[] which)
        {
            // khi đọc ra null/hỏng, refresh cache & resolve lại đúng handle
            _automationService2.RefreshCache();
            _pidEl = _pidEl ?? _automationService2.GetControlById(_config2.Controls.PID);
            _resultEl = _resultEl ?? _automationService2.GetChildControl(_config2.Controls.ResultUc, _config2.Controls.ResultText);
            _ebrEl = _ebrEl ?? _automationService2.GetChildControl(_config2.Controls.ModelSuffixUc, "txtText");
            _woEl = _woEl ?? _automationService2.GetChildControl(_config2.Controls.WorkOrderUc, "txtText");
            _qtyEl = _qtyEl ?? _automationService2.GetControlById(_config2.Controls.ResultQty);
            _resultUcEl = _resultUcEl ?? _automationService2.GetControlById(_config2.Controls.ResultUc);
        }


        private void ResetRtxtDetailExplain()
        {
            rtxtDetailExplain.Clear();
            rtxtDetailExplain.ForeColor = Color.Black;
            rtxtDetailExplain.BackColor = SystemColors.Window;
            rtxtDetailExplain.Font = new Font(rtxtDetailExplain.Font.FontFamily, 9, FontStyle.Regular);
        }
        private void ShowNoticeAndExplain(List<string> msgs)
        {
            if (msgs.Count != 0)
            {
                rtxtDetailExplain.Clear();
                foreach (var msg in msgs)
                {
                    var res = _translator.TranslateAndGuide(msg);
                    lbDetailStatus.Text = _translator.Translate(res.Translated);
                    rtxtDetailExplain.AppendText($"\u2605 {res.ExplainEs}\r\n");
                    rtxtDetailExplain.SelectionStart = richTextBox1.TextLength;
                    rtxtDetailExplain.ScrollToCaret();
                }
            }
        }

        // ======= helpers =======

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, bool wParam, int lParam);
        const int WM_SETREDRAW = 0x000B;

        sealed class RedrawScope : IDisposable
        {
            private readonly Control _ctrl;
            public RedrawScope(Control ctrl)
            {
                _ctrl = ctrl;
                if (ctrl?.IsHandleCreated == true)
                    SendMessage(ctrl.Handle, WM_SETREDRAW, false, 0);
            }
            public void Dispose()
            {
                if (_ctrl?.IsHandleCreated == true)
                {
                    SendMessage(_ctrl.Handle, WM_SETREDRAW, true, 0);
                    _ctrl.Invalidate(true);
                    _ctrl.Update();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetIfChanged(Label lbl, string text)
        {
            var t = text ?? string.Empty;
            if (!string.Equals(lbl.Text, t, StringComparison.OrdinalIgnoreCase))
            { lbl.Text = t; return true; }
            return false;
        }
     

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetIfChanged(System.Windows.Forms.TextBox tb, string text)
        {
            var t = text ?? string.Empty;
            if (!string.Equals(tb.Text, t, StringComparison.OrdinalIgnoreCase))
            { tb.Text = t; return true; }
            return false;
        }

        // Khai báo static field
        private static System.Media.SoundPlayer _okPlayer;
        private static System.Media.SoundPlayer _ngPlayer;
        private static volatile bool _soundReady;
        private static readonly object _soundLock = new();

        // Hàm đảm bảo nạp resource chỉ 1 lần
        private void EnsureSoundsLoaded()
        {
            if (_soundReady) return;
            lock (_soundLock)
            {
                if (_soundReady) return;

                // Dùng resource nhúng (Properties.Resources.PASS/FAIL là byte[])
                _okPlayer = new System.Media.SoundPlayer(new MemoryStream(Properties.Resources.PASS));
                _ngPlayer = new System.Media.SoundPlayer(new MemoryStream(Properties.Resources.FAIL));

                _okPlayer.Load();   // preload vào RAM
                _ngPlayer.Load();

                _soundReady = true;
            }
        }

        // Hàm phát âm thanh (đã preload)
        private long _lastSoundTick;
        private const int SoundMinGapMs = 120; // tránh phát 2 lần dính nhau

        private void PlaySoundOk()
        {
            EnsureSoundsLoaded();
            var now = Environment.TickCount64;
            if (now - _lastSoundTick >= SoundMinGapMs)
            {
                _okPlayer?.Play();
                _lastSoundTick = now;
            }
        }

        private void PlaySoundNg()
        {
            EnsureSoundsLoaded();
            var now = Environment.TickCount64;
            if (now - _lastSoundTick >= SoundMinGapMs)
            {
                _ngPlayer?.Play();
                _lastSoundTick = now;
            }
        }



        // tránh Update chart nặng khi không đổi
        private (int done, int total) _lastProgress = (-1, -1);

        // ======= UpdateUI tối ưu (UI-thread) =======

        private void UpdateUI(PCB data)
        {
            if (data == null) return;
            if (!this.IsHandleCreated || this.IsDisposed) return;

            // Giảm repaint trong khi set nhiều control
            using (new RedrawScope(this))
            {
                // 1) Chuẩn hoá chuỗi
                var resultRaw = data.Result ?? string.Empty;
                var resultClean = resultRaw.Trim();
                var ebrCur = (data.EBR ?? string.Empty).Trim();
                var ebrSetting = (_settingEBR ?? string.Empty).Trim();

                // 2) Phân loại
                var kind = Classify(resultClean);

                // 3) EBR mismatch → ép NG khi OK
                bool hasSetting = !string.IsNullOrWhiteSpace(ebrSetting);
                bool mismatch = hasSetting && !string.Equals(ebrCur, ebrSetting, StringComparison.OrdinalIgnoreCase);
                var finalKind = (kind == ResultKind.Ok && mismatch) ? ResultKind.Ng : kind;

                // 4) Chỉ Apply style khi khác lần trước (nếu bạn có lưu trạng thái)
                ApplyStatusStyle(finalKind);

                // 5) Gắn EBR setting lần đầu(chỉ khi khác)
                if (kind == ResultKind.Ok && string.IsNullOrWhiteSpace(txtSettingEBR.Text))
                {
                    if (SetIfChanged(txtSettingEBR, ebrCur))
                    {
                        _settingEBR = ebrCur;
                    }
                }
                //if (SetIfChanged(txtSettingEBR, ebrCur))
                //{
                //    _settingEBR = ebrCur;
                //}
                List<string> msgs = new List<string>();
                // 6) Render khi có PID
                if (!string.IsNullOrWhiteSpace(data.PID))
                {
                    // 6.1) Cảnh báo mismatch: chỉ append khi thực sự có mismatch
                    if (mismatch)
                    {
                        // Tránh Reset + Append liên tục: chỉ reset khi nội dung khác
                        var warn = $"⚠️ P/No escaneado ({ebrCur}) no coincide con P/No configurado ({ebrSetting}) → Por favor clasificar en el lugar correcto.";
                        //ShowNoticeAndExplain(warn);
                        msgs.Add(warn);

                    }

                    // 6.2) Âm thanh: đã preload + debounce
                    if (finalKind == ResultKind.Ng) PlaySoundNg();
                    else if (finalKind == ResultKind.Ok) PlaySoundOk();

                    // 6.3) Left panel (chỉ set khi khác để không trigger layout)
                    SetIfChanged(lbModelSuffixData, ebrCur);
                    SetIfChanged(lbWoData, data.WO ?? string.Empty);
                    SetIfChanged(lbBuyerData, data.Buyer ?? string.Empty);

                    // 6.4) Right panel
                    SetIfChanged(lbPID, $"PCBA S/N: {data.PID}");
                    // thể hiện rõ “OK có lệch EBR”
                    var statusText = mismatch ? "OK (Desajuste de EBR)" : resultRaw;
                    SetIfChanged(lbStatus, statusText);
                    // Set preview
                    SetIfChanged(lbPreviewPno, data.EBR);
                    SetIfChanged(lbPreviewScanQty, _pendingPids.Count().ToString());

                    // 6.5) Message detail – chỉ viết khi có & khác
                    if (!string.IsNullOrWhiteSpace(data.Message)) msgs.Add(data.Message);

                    ShowNoticeAndExplain(msgs);
                    //ShowNoticeAndExplain(data.Message);
                    // 6.6) Progress – chỉ cập nhật khi OK (không mismatch) **và** giá trị khác
                    if (finalKind == ResultKind.Ok)
                    {
                        var (completedQty, woQty) = ExtractProgressSafe(data.Progress);
                        int done = Math.Max(0, completedQty);
                        int total = Math.Max(1, woQty);
                        var remain = Math.Max(0, total - done);

                        // Set label nếu khác
                        if (!string.Equals(lbRemainQtyData.Text, remain.ToString(), StringComparison.Ordinal))
                            lbRemainQtyData.Text = remain.ToString();

                        // Check thay đổi trước khi vẽ chart
                        if (_lastProgress.done != done || _lastProgress.total != total)
                        {
                            _lastProgress = (done, total);
                            // nếu SetWorkOrderProgress nặng, bọc thêm SuspendLayout/ResumeLayout của chart
                            chartProgress.SuspendLayout();
                            try
                            {
                                SetWorkOrderProgress(chartProgress, lbProgressData, done, total);
                            }
                            finally
                            {
                                chartProgress.ResumeLayout();
                            }
                        }
                    }
                }
            }
        }

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
                txtSettingEBR.Clear();

                // Clear prevew
                lbPreviewPno.Text = "";
                lbPreviewScanQty.Text = "";

                // Clear data
                // Reset biến/UI
                _tagLabel = new ManufacturingTagDto();
                _lastPidValue = string.Empty;
                _lastStatusValue = string.Empty;
                _settingEBR = string.Empty;
                lock (_pendingLock)
                {
                    _pendingPids.Clear();
                }


                // Clear lỗi trên SFC
                _automationService2.ClickTo(_config2.Controls.ClearButton);

            }
            catch
            {

                MessageBox.Show("No se puede enviar la operación, abra el software SFC.LGE");
            }
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



        #region Task upload PID lên DB

        private async Task UploadToDb(PCB data, CancellationToken ct = default)
        {
            AddLog($"[DEBUG] Snap Data");
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

                AddLog($"[DEBUG]Check exist");
                // Check tồn tại theo PID
                bool exists = await db.TbScanOuts
                    .AnyAsync(x => x.Pid == data.PID, ct)
                    .ConfigureAwait(false);

                if (exists)
                {
                    AddLog($"[DEBUG]PID exist, Ignore");
                    return;
                }

                AddLog($"[DEBUG]No exist, start insert");
                await db.TbScanOuts.AddAsync(scanOutData, ct).ConfigureAwait(false);
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                AddLog($"[DEBUG] Finish insert");
                lock (_pendingLock)
                {
                    _pendingPids.Add(data.PID);
                }
                AddLog($"[DEBUG] Add to _pendingPids");


            }
            catch (Exception ex)
            {
                // log hoặc hiện thông báo (UI thông báo nên đưa qua OnUiAsync)
                AddLog("DB error: " + ex.Message);
                // tùy nhu cầu: rethrow hoặc nuốt
                // throw;
            }
        }
        private UploadBackgroundService _uploadSvc;

        private void InitUploadService()
        {
            _uploadSvc = new UploadBackgroundService(
                uploadFunc: async (snap, ct) =>
                {
                    // chính là UploadToDb của bạn (KHÔNG chạm UI ở đây)
                    await UploadToDb(snap, ct).ConfigureAwait(false);
                },
                maxDegreeOfParallelism: 2 // muốn 1 → chạy lần lượt; >1 → song song
            );
        }




        #endregion

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

        private async void btnPrint_Click(object sender, EventArgs e)
        {
            var msg = string.Empty;
            if (_uploadSvc.IsAnyRunning())
            {
                msg = "Sincronizando servidor, por favor espere";
                ShowDetailStatus(msg);
                await _uploadSvc.WaitAllIdleAsync();
            }

            msg = "Sincronización completada. Comienza a imprimir sellos.";
            ShowDetailStatus(msg);
            try
            {
                //if (!lbStatus.Text.Equals("OK", StringComparison.OrdinalIgnoreCase))
                //{
                //    ShowDetailStatus("El último PID escaneado debe ser OK y coincidir con EBR.");
                //    return;
                //}

                // snapshot danh sách PID đang chờ in (batch hiện tại)
                string[] batchPids;
                lock (_pendingLock)
                {
                    batchPids = _pendingPids.ToArray();
                }
                AddLog($"[DEBUG] batchPids: {string.Join(",\r\n", batchPids)}");
                AddLog($"[DEBUG] _ClientIp: {_ClientIp}, _settingEBR: {_settingEBR}");

                // And after fetching tagDatas:

                if (batchPids.Length == 0)
                {
                    ShowDetailStatus("No hay PIDs pendientes para imprimir.");
                    return;
                }

                using var db = await _dbFactory.CreateDbContextAsync();

                // Lấy đúng các dòng thuộc batch hiện tại, chưa in
                var tagDatas = await (
                    from s in db.TbScanOuts
                    join m in db.TbModelDicts on s.PartNo equals m.PartNo into gj
                    from m in gj.DefaultIfEmpty()     // <-- left join
                    where batchPids.Contains(s.Pid)
                    select new
                    {
                        PartNo = m != null ? m.PartNo : null,   // có thể null nếu không match
                        ModelName = m != null ? m.ModelName : null,
                        Board = m != null ? m.Board : null,
                        Pid = s.Pid
                    }
                ).ToListAsync();

                AddLog($"[DEBUG] tagDatas.Count: {tagDatas.Count}");
                if (tagDatas.Count == 0)
                {
                    ShowDetailStatus("No se encontraron PIDs válidos en el lote actual.");
                    //// loại các PID không còn match khỏi batch để tránh lặp vô hạn
                    //lock (_pendingLock)
                    //{
                    //    foreach (var pid in batchPids) _pendingPids.Remove(pid);
                    //}
                    return;
                }

                var now = DateTime.Now;
                long tagId = GenerateTagId();

                //AddLog("Assign data for tag label\r\n");
                _tagLabel = new ManufacturingTagDto
                {
                    ModelName = $"{tagDatas[0].ModelName}-{tagDatas[0].Board}",
                    Date = now.ToString("dd/MM/yyyy"),
                    PartNo = tagDatas[0].PartNo,
                    Quantity = tagDatas.Count,
                    TagId = tagId,
                    PidList = tagDatas.Select(x => x.Pid).ToList()
                };

                //AddLog("Print\r\n");
                PrintTag(_tagLabel);

                AddLog($"[DEBUG] Start update database");
                await db.TbScanOuts
                    .Where(x => _tagLabel.PidList.Contains(x.Pid))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.TagId, r => tagId)
                        .SetProperty(r => r.PrintAt, r => DateTime.Now)
                        .SetProperty(r => r.PrintDate, r => DateOnly.FromDateTime(DateTime.Now))
                    );
                AddLog($"[DEBUG] Start reset all data");

                resetUI();

                AddLog($"[DEBUG] Completed! Reset all data");
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
            try
            {
                AddLog("PrintTag: Start printing tag.");
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
                AddLog("PrintTag: Start generate label...");
                _tagLabel = tag;
                doc.PrintPage += GenerateLabel_FitMarginBounds;

                AddLog("PrintTag: Calling doc.Print()...");
                doc.Print();
                AddLog("PrintTag: Print completed.");
            }
            catch (Exception ex)
            {
                AddLog($"PrintTag: Exception - {ex.Message}\r\n{ex.StackTrace}");
                throw;
            }
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
                // Đảm bảo luôn chạy khi app start hoặc khi người dùng click
                bool needSetup = cbtnConfirmSetting.Checked || !_isWatching;

                if (needSetup)
                {
                    lbDetailStatus.Text = "Instalando. Por favor espere....";
                    lbDetailStatus.ForeColor = Color.Yellow;
                    txtInspector1.Enabled = false;
                    txtInspector2.Enabled = false;
                    txtSettingEBR.Enabled = false;
                    int maxRetry = 50;
                    int delayMs = 200;
                    bool attached = false, canConnect = false;

                    string dbMsg = string.Empty;
                    string SfcMsg = string.Empty;

                    _inspector1 = Normalize(txtInspector1.Text);
                    _inspector2 = Normalize(txtInspector2.Text);
                    for (int attempt = 1; attempt <= maxRetry; attempt++)
                    {
                        // Check DB connection
                        using var db = await _dbFactory.CreateDbContextAsync();
                        canConnect = await db.Database.CanConnectAsync();
                        if (!canConnect)
                        {
                            dbMsg = "No se pudo conectar a la base de datos.";
                            AddLog($"============ Connect to DB fail ============");
                            await Task.Delay(delayMs);
                            continue;
                        }


                        // Check SFC attach
                        try
                        {
                            _automationService2.AttachToProcess(_config2.ProcessName);
                            attached = true;
                        }
                        catch (Exception ex)
                        {
                            AddLog($"============ Retry {attempt}/{maxRetry} ============");
                            AddLog(ex.ToString());
                            await Task.Delay(delayMs);
                            continue;
                        }

                        // If both connected & attached -> break    
                        if (attached && canConnect)
                        {
                            dbMsg = "DB conectado!";
                            SfcMsg = "Vinculado exitosamente con SFC (OK)";
                        }
                        break;

                    }



                    if (!attached || !canConnect)
                    {
                        lbDetailStatus.ForeColor = Color.Yellow;
                        txtInspector1.Enabled = true;
                        txtInspector2.Enabled = true;
                        txtSettingEBR.Enabled = true;
                        return;
                    }




                    try
                    {
                        resetUI(); // reset all to default
                        lbDetailStatus.Text = $"{SfcMsg} | {dbMsg}";
                        lbDetailStatus.ForeColor = Color.Lime;
                    }
                    catch (Exception ex)
                    {
                        AddLog(ex.Message);
                        txtInspector1.Enabled = true;
                        txtInspector2.Enabled = true;
                        txtSettingEBR.Enabled = true;
                        return;
                    }

                    if (!_isWatching)
                    {
                        StartWatchingPidPollingTask();
                        _isWatching = true;
                    }

                    txtInspector1.Enabled = false;
                    txtInspector2.Enabled = false;
                    txtSettingEBR.Enabled = false;
                }
                else
                {
                    resetUI(); // reset all to default
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
                        AddLog($"Error: {ex.Message}");
                    }
                    finally
                    {
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
                    //rtxtDetailExplain.AppendText($"{msg}\r\n");
                    richTextBox1.AppendText($"{msg}\r\n");
                    richTextBox1.SelectionStart = richTextBox1.TextLength;
                    richTextBox1.ScrollToCaret();
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

        /// <summary>
        /// Cải tiến lần 2
        /// </summary>
        /// <param name="maxTries"></param>
        /// <param name="delayMs"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        //Stopwatch sw = new Stopwatch();
        //private async Task LoopAsync(CancellationToken ct)
        //{

        //    var sw = Stopwatch.StartNew();
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
        //            sw = Stopwatch.StartNew();

        //            // === 1) ĐỌC SNAPSHOT 1 LẦN TRÊN UI THREAD ===
        //            _building = true;
        //            var data = await OnUiAsync(() =>
        //            {
        //                try
        //                {
        //                    string pid = Normalize(_automationService2.GetText(_config2.Controls.PID));
        //                    string result = Normalize(_automationService2.GetText(_config2.Controls.ResultUc, _config2.Controls.ResultText));
        //                    string ebr = Normalize(_automationService2.GetText(_config2.Controls.ModelSuffixUc, "txtText"));
        //                    string wo = Normalize(_automationService2.GetText(_config2.Controls.WorkOrderUc, "txtText"));
        //                    string msg = Normalize(_automationService2.GetInnerMessageFromResult(_config2.Controls.ResultUc, _config2.Controls.ResultText));
        //                    string prog = Normalize(_automationService2.GetText(_config2.Controls.ResultQty));

        //                    if (string.IsNullOrWhiteSpace(pid)) return null;

        //                    return new PCB
        //                    {
        //                        PID = pid,
        //                        Result = result,
        //                        EBR = ebr,
        //                        WO = wo,
        //                        Message = msg,
        //                        Progress = prog
        //                    };
        //                }
        //                catch (Exception ex)
        //                {
        //                    AddLog("OnUi snapshot error: " + ex.Message);
        //                    return null;
        //                }
        //            }).ConfigureAwait(false);
        //            sw.Stop();
        //            AddLog($"Read UI:{sw.ElapsedMilliseconds}");

        //            if (data == null)
        //            {
        //                _building = false;
        //                await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
        //                continue;
        //            }



        //            // Dedup: nếu PID & Result (clean) trùng với lần trước → bỏ qua
        //            var resClean = Normalize(data.Result);
        //            if (string.Equals(data.PID, _lastPidValue, StringComparison.OrdinalIgnoreCase) &&
        //                string.Equals(resClean, _lastStatusValue, StringComparison.OrdinalIgnoreCase))
        //            {
        //                _building = false;
        //                await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
        //                continue;
        //            }
        //             sw = Stopwatch.StartNew();

        //            // === 2) DEBOUNCE/ỔN ĐỊNH TRẠNG THÁI RESULT ===
        //            // Nếu chưa OK, chờ ngắn rồi đọc lại 1-2 lần để tránh miss khung READY→OK
        //            if (Classify(resClean) != ResultKind.Ok)
        //            {
        //                var confirmed = await WaitStableOkAsync(maxTries: 2, delayMs: 60, ct);
        //                if (confirmed != null)
        //                {
        //                    // cập nhật theo snapshot OK
        //                    data.Result = confirmed.Result;
        //                    data.Message = confirmed.Message;
        //                    data.Progress = confirmed.Progress;
        //                    resClean = Normalize(data.Result);
        //                }
        //            }
        //            sw.Stop();
        //            AddLog($"WaitStableOkAsync:{sw.ElapsedMilliseconds}");

        //            // === 3) CẬP NHẬT UI ===
        //             sw = Stopwatch.StartNew();
        //            await OnUiAsync(() =>
        //            {
        //                UpdateUI(data);
        //                return 0;
        //            }).ConfigureAwait(false);
        //            sw.Stop();
        //            AddLog($"UpdateUI:{sw.ElapsedMilliseconds}");

        //            // Cập nhật mốc dedup bằng GIÁ TRỊ ĐÃ CLEAN (nhất quán với Classify)
        //            _lastPidValue = data.PID;
        //            _lastStatusValue = resClean;
        //             sw = Stopwatch.StartNew();
        //            // === 4) UPLOAD DB KHI OK (sau khi đã debounce) ===
        //            if (Classify(resClean) == ResultKind.Ok)
        //            {
        //                try
        //                {
        //                    // snapshot tối thiểu cho DB
        //                    var snap = new PCB
        //                    {
        //                        PID = data.PID,
        //                        EBR = data.EBR,
        //                        WO = data.WO,
        //                        Result = data.Result,
        //                        Message = data.Message,
        //                        Progress = data.Progress
        //                    };
        //                    await UploadToDb(snap, ct).ConfigureAwait(false);
        //                }
        //                catch (OperationCanceledException) { }
        //                catch (Exception ex)
        //                {
        //                    AddLog("UploadToDb error: " + ex.Message);
        //                }
        //            }
        //            sw.Stop();
        //            AddLog($"UploadToDb:{sw.ElapsedMilliseconds}");

        //            _building = false;
        //        }
        //        catch (OperationCanceledException) { break; }
        //        catch (Exception ex)
        //        {
        //            AddLog("Loop error: " + ex);
        //            _building = false;
        //            await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
        //        }
        //    }
        //}


        private async Task LoopAsync(CancellationToken ct)
        {
            const int idleDelayMs = 100;
            const int busyDelayMs = 40; // nhạy hơn chút

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_building)
                    {
                        await Task.Delay(busyDelayMs, ct).ConfigureAwait(false);
                        continue;
                    }

                    _building = true;

                    // ===== 1A) HEAD: PID + RESULT =====
                    var head = await _uiaWorker.Run(() =>
                    {
                        EnsureHandles();

                        var inner = new System.Text.StringBuilder();

                        string pid = Normalize(_automationService2.SafeGetText(_pidEl));

                        string result = Normalize(_automationService2.SafeGetText(_resultEl));

                        // nếu null/empty bất thường -> thử refresh handle 1 lần
                        if (string.IsNullOrEmpty(pid) && string.IsNullOrEmpty(result))
                        {
                            InvalidateHandlesIfNulls();
                            pid = Normalize(_automationService2.SafeGetText(_pidEl));
                            result = Normalize(_automationService2.SafeGetText(_resultEl));
                            inner.Append("[refreshed] ");
                        }

                        return (pid, result);
                    }).ConfigureAwait(false);

                    //// retry siêu ngắn nếu bị spike
                    //if (sw.ElapsedMilliseconds > 120)
                    //{
                    //    var swRetry = Stopwatch.StartNew();
                    //    head = await _uiaWorker.Run(() =>
                    //    {
                    //        string pid2 = Normalize(_automationService2.SafeGetText(_pidEl));
                    //        string result2 = Normalize(_automationService2.SafeGetText(_resultEl));
                    //        return (pid2, result2);
                    //    });
                    //    swRetry.Stop();
                    //    AddLog($"Read UI (head) RETRY:{swRetry.ElapsedMilliseconds}");
                    //}



                    if (string.IsNullOrWhiteSpace(head.pid))
                    {
                        _building = false;
                        await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
                        continue;
                    }

                    // Dedup sớm
                    var resClean = Normalize(head.result);
                    if (string.Equals(head.pid, _lastPidValue, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(resClean, _lastStatusValue, StringComparison.OrdinalIgnoreCase))
                    {
                        _building = false;
                        await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
                        continue;
                    }

                    // ===================== 2) DEBOUNCE NHẸ (nếu chưa OK) =====================
                    if (Classify(resClean) != ResultKind.Ok)
                    {
                        var confirmed = await WaitStableOkAsync(maxTries: 2, delayMs: 60, ct);
                        if (confirmed != null)
                        {
                            // cập nhật theo snapshot OK
                            resClean = Normalize(confirmed.Result);
                            head = (head.pid, resClean);
                        }
                    }

                    // ===================== 1B) ĐỌC SÂU KHI CẦN =====================
                    // Chỉ đọc EBR/WO/Progress… khi đã chắc chắn có thay đổi PID/Result.
                    // ===================== 1B) ĐỌC SÂU KHI CẦN =====================
                    var tail = await _uiaWorker.Run(() =>
                    {
                        var inner = new System.Text.StringBuilder();

                        string ebr = Normalize(_automationService2.GetText(_config2.Controls.ModelSuffixUc, "txtText"));

                        string wo = Normalize(_automationService2.GetText(_config2.Controls.WorkOrderUc, "txtText"));

                        string prog = Normalize(_automationService2.GetText(_config2.Controls.ResultQty));

                        string msg = null;
                        if (Classify(resClean) == ResultKind.Ng)
                        {
                            msg = Normalize(_automationService2.GetInnerMessageFromResult(_config2.Controls.ResultUc, _config2.Controls.ResultText));
                        }

                        return (ebr, wo, prog, msg);
                    }).ConfigureAwait(false);


                    var data = new PCB
                    {
                        PID = head.pid,
                        Result = resClean,
                        EBR = tail.ebr,
                        WO = tail.wo,
                        Message = tail.msg,
                        Progress = tail.prog
                    };

                    // ===================== 3) CẬP NHẬT UI (chỉ khi khác) =====================
                    if (!UiStateEquals(_lastUi, data))
                    {
                        var swUpdate = Stopwatch.StartNew();
                        await OnUiAsync(() => { UpdateUI(data); return 0; }).ConfigureAwait(false);
                        swUpdate.Stop();
                        _lastUi = data;
                    }


                    // Ghi mốc dedup
                    _lastPidValue = data.PID;
                    _lastStatusValue = data.Result;

                    // ===================== 4) GHI DB – CHẠY NỀN =====================
                    if (Classify(resClean) == ResultKind.Ok)
                    {
                        var snap = new PCB
                        {
                            PID = data.PID,
                            EBR = data.EBR,
                            WO = data.WO,
                            Result = data.Result,
                            Message = data.Message,
                            Progress = data.Progress
                        };
                        _uploadSvc.Enqueue(snap); // fire-and-forget, chạy nền
                    }
                    _building = false;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AddLog("Loop error: " + ex);
                    _building = false;
                    await Task.Delay(idleDelayMs, ct).ConfigureAwait(false);
                }
            }
        }

        // ===================== Helper nhỏ (nếu bạn cần) =====================
        private bool UiStateEquals(PCB a, PCB b)
        {
            if (a == null || b == null) return false;
            return string.Equals(a.PID, b.PID, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Result, b.Result, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.EBR, b.EBR, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.WO, b.WO, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Message, b.Message, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Progress, b.Progress, StringComparison.OrdinalIgnoreCase);
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

            if (_uploadSvc != null)
                await _uploadSvc.DisposeAsync();
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

        private void SetAutoStart(bool enable)
        {
            try
            {
                string appName = System.Windows.Forms.Application.ProductName;
                string exePath = System.Windows.Forms.Application.ExecutablePath;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (enable)
                    {
                        key.SetValue(appName, exePath);
                        AddLog("Added to auto start");
                    }

                    else
                        key.DeleteValue(appName, false);
                }
            }
            catch (Exception ex)
            {
                AddLog("SetAutoStart error: " + ex.Message);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
    // 1) Helper: STA worker (một thread STA có queue)
    public sealed class StaWorker : IDisposable
    {
        private readonly System.Collections.Concurrent.BlockingCollection<Action> _q
            = new System.Collections.Concurrent.BlockingCollection<Action>();
        private readonly Thread _thread;

        public StaWorker(string name = "UIA-STA")
        {
            _thread = new Thread(() =>
            {
                // STA + message loop không bắt buộc, nhưng nên có
                System.Windows.Forms.Application.Idle += (_, __) => { };
                foreach (var act in _q.GetConsumingEnumerable())
                {
                    try { act(); } catch { /* swallow để không kill thread */ }
                }
            })
            {
                IsBackground = true,
                Name = name
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public Task Run(Action action)
        {
            var tcs = new TaskCompletionSource<object?>();
            _q.Add(() =>
            {
                try { action(); tcs.SetResult(null); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }

        public Task<T> Run<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            _q.Add(() =>
            {
                try { tcs.SetResult(func()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }

        public void Dispose() => _q.CompleteAdding();
    }

    /// <summary>
    /// Chạy UploadToDb ở background (không chặn UI), theo dõi trạng thái để caller
    /// có thể kiểm tra/đợi rồi *tự gọi* PrintTag() khi sẵn sàng.
    /// </summary>
    public sealed class UploadBackgroundService : IAsyncDisposable
    {
        private readonly Channel<PCB> _queue;
        private readonly Func<PCB, CancellationToken, Task> _uploadFunc;
        private readonly ConcurrentDictionary<string, Task> _tasksByPid = new();
        private readonly SemaphoreSlim _parallelLimiter;
        private readonly CancellationTokenSource _cts = new();
        private readonly List<Task> _workers = new();
        private int _inFlight;

        /// <param name="uploadFunc">Hàm thực hiện upload (không chạm UI)</param>
        /// <param name="maxDegreeOfParallelism">Số upload chạy song song (mặc định 2)</param>
        public UploadBackgroundService(Func<PCB, CancellationToken, Task> uploadFunc, int maxDegreeOfParallelism = 2)
        {
            _uploadFunc = uploadFunc ?? throw new ArgumentNullException(nameof(uploadFunc));
            _queue = Channel.CreateUnbounded<PCB>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });
            _parallelLimiter = new SemaphoreSlim(Math.Max(1, maxDegreeOfParallelism), Math.Max(1, maxDegreeOfParallelism));

            // tạo các worker đọc hàng đợi
            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                _workers.Add(Task.Run(WorkerLoopAsync));
            }
        }

        public bool Enqueue(PCB snap)
        {
            if (snap == null || string.IsNullOrWhiteSpace(snap.PID)) return false;
            return _queue.Writer.TryWrite(snap);
        }

        public async Task EnqueueAsync(PCB snap, CancellationToken ct = default)
        {
            if (snap == null || string.IsNullOrWhiteSpace(snap.PID)) return;
            await _queue.Writer.WriteAsync(snap, ct).ConfigureAwait(false);
        }

        /// <summary> Có bất kỳ upload nào đang chạy không? </summary>
        public bool IsAnyRunning() => Volatile.Read(ref _inFlight) > 0;

        /// <summary> Upload của PID này còn đang chạy không? </summary>
        public bool IsRunning(string pid)
            => !string.IsNullOrWhiteSpace(pid) &&
               _tasksByPid.TryGetValue(pid, out var t) &&
               t is { IsCompleted: false };

        /// <summary> Đợi upload của PID này xong (optional: timeout). </summary>
        public async Task WaitForPidAsync(string pid, TimeSpan? timeout = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(pid)) return;
            if (_tasksByPid.TryGetValue(pid, out var t))
            {
                if (timeout is { } to && to > TimeSpan.Zero)
                    await Task.WhenAny(t, Task.Delay(to, ct)).ConfigureAwait(false);
                else
                    await t.ConfigureAwait(false);
            }
        }

        /// <summary> Đợi đến khi không còn upload nào đang chạy. </summary>
        public async Task WaitAllIdleAsync(CancellationToken ct = default)
        {
            // snapshot tránh race
            var running = _tasksByPid.Values.Where(x => !x.IsCompleted).ToArray();
            if (running.Length > 0)
                await Task.WhenAll(running).ConfigureAwait(false);
        }

        private async Task WorkerLoopAsync()
        {
            try
            {
                await foreach (var snap in _queue.Reader.ReadAllAsync(_cts.Token))
                {
                    await _parallelLimiter.WaitAsync(_cts.Token).ConfigureAwait(false);

                    // mỗi item xử lý trong 1 task để worker lấy tiếp item khác
                    _ = Task.Run(async () =>
                    {
                        Interlocked.Increment(ref _inFlight);
                        var pid = snap.PID!;
                        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                        _tasksByPid[pid] = tcs.Task;

                        try
                        {
                            await _uploadFunc(snap, _cts.Token).ConfigureAwait(false);
                            tcs.TrySetResult(null); // đánh dấu xong pid này
                        }
                        catch (OperationCanceledException)
                        {
                            tcs.TrySetCanceled();
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                        finally
                        {
                            _tasksByPid.TryRemove(pid, out _);
                            Interlocked.Decrement(ref _inFlight);
                            _parallelLimiter.Release();
                        }
                    }, _cts.Token);
                }
            }
            catch (OperationCanceledException) { /* normal on dispose */ }
        }

        public async ValueTask DisposeAsync()
        {
            _queue.Writer.TryComplete();
            _cts.Cancel();
            try { await Task.WhenAll(_workers).ConfigureAwait(false); } catch { }
            _parallelLimiter.Dispose();
            _cts.Dispose();
        }
    }
    
}
