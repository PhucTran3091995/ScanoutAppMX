using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MSFC.Data;
using MSFC.Models;
using MSFC.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace MSFC
{
    public partial class frmKLAS : Form
    {
        private readonly IDbContextFactory<MMesDbContext> _dbFactory;
        //========== UI automation theo GMES 2.0 ==========
        private IAutomationService2 _automationService2;
        private readonly KlasUiConfig _Config;
        private readonly StaWorker _uiaWorker = new StaWorker();
        // Field guard để chống reentrancy & track watcher
        private bool _isWatching = false;
        private CancellationTokenSource? _cts;
        private Task? _mainLoopTask;
        private Task? _keepAliveTask;
        private bool _isLogin = false;
        private int _interval;
        private NotifyIcon _notifyIcon;

        public frmKLAS(
        IDbContextFactory<MMesDbContext> dbFactory,
        IAutomationService2 automationService2,
        IOptions<KlasUiConfig> klasConfig)
        {
            _dbFactory = dbFactory;

            //========== UI automation theo GMES 2.0 ==========
            _automationService2 = automationService2;
            _Config = klasConfig.Value;
            InitializeComponent();
            InitTrayIcon();
        }
        private void AddLog(string msg)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() =>
                {
                    var lastIndex = rtxtLog.Lines.Length;
                    //rtxtDetailExplain.AppendText($"{msg}\r\n");
                    rtxtLog.AppendText($"{lastIndex}. {msg}\r\n");
                    rtxtLog.SelectionStart = rtxtLog.TextLength;
                    rtxtLog.ScrollToCaret();
                }));
            }
        }

        private void InitTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = this.Icon, // dùng icon của form
                Text = "KLAS Clone Data App",
                Visible = false
            };

            // Menu chuột phải (nếu muốn)
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open", null, (s, e) => RestoreFromTray());
            contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
            _notifyIcon.ContextMenuStrip = contextMenu;

            // Double-click icon => restore
            _notifyIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
            _notifyIcon.Visible = false;
        }
        private bool OpenKLAS()
        {
            try
            {
                string? appPath = _Config.AppUrl;

                if (string.IsNullOrWhiteSpace(appPath))
                {
                    AddLog("⚠️ AppUrl not configured in appsettings.json.");
                    return false;
                }

                if (!File.Exists(appPath))
                {
                    AddLog($"❌ App not found: {appPath}");
                    return false;
                }

                // Kiểm tra nếu process đã chạy
                string exeName = Path.GetFileNameWithoutExtension(appPath);
                var existing = Process.GetProcessesByName(exeName);
                if (existing.Length > 0)
                {
                    AddLog($"ℹ️ App '{exeName}' is already running.");
                    return true;
                }

                // Mở app
                AddLog($"🚀 Launching app: {appPath}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = appPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(appPath)!
                });
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"❌ Failed to open app: {ex.Message}");
                return false;
            }
        }
        private readonly SemaphoreSlim _gate = new(1, 1);

        private async void btnStart_Click(object sender, EventArgs e)
        {
            await _gate.WaitAsync();
            try
            {
                btnStart.Text = "Stop";
                btnStart.Enabled = false;          // chống double-click trong lúc chuyển trạng thái
                numInterval.Enabled = false;

                if (!canConnect)
                {
                    for (int attempt = 1; attempt <= 5; attempt++)
                    {
                        AddLog($"============ Try to connect DB {attempt}/{5} ============");
                        using var db = await _dbFactory.CreateDbContextAsync();
                        canConnect = await db.Database.CanConnectAsync();
                        if (canConnect) { AddLog("Connected to DB successfully!"); break; }

                        await Task.Delay(500);
                        continue;
                    }
                    if (!canConnect)
                    {
                        btnStart.Enabled = true;          // chống double-click trong lúc chuyển trạng thái
                        numInterval.Enabled = true;
                        btnStart.Text = "Start";
                        AddLog("Can't connect to DB! Recheck connection or setting");
                        return;
                    }
                }
                else AddLog("Connected to DB successfully!");


                if (!_isWatching)
                    await StartAsync();
                else
                    await StopAsync();
            }
            catch (Exception ex)
            {
                AddLog($"❌ Toggle error: {ex.Message}");
            }
            finally
            {
                btnStart.Enabled = true;
                numInterval.Enabled = !_isWatching; // chỉ cho đổi interval khi không chạy
                btnStart.Text = _isWatching ? "Stop" : "Start";
                _gate.Release();
            }
        }
        private async Task StartAsync()
        {
            // Nếu còn task cũ => dừng gọn trước
            if (_cts != null || _mainLoopTask != null)
                await StopAsync();

            _cts = new CancellationTokenSource();
            _isWatching = true;

            // KHÔNG cần Task.Run: LoopAsync tự await bên trong, không block UI thread
            _mainLoopTask = LoopAsync(_cts.Token);
            AddLog("▶️ Started.");
        }
        private async Task StopAsync()
        {
            if (_cts == null && _mainLoopTask == null)
                return;

            try
            {
                btnStart.Enabled = true;          // chống double-click trong lúc chuyển trạng thái
                numInterval.Enabled = true;
                btnStart.Text = "Start";
                _cts?.Cancel();
                if (_mainLoopTask != null)
                    await _mainLoopTask; // đợi vòng lặp thoát hẳn
                AddLog("⏹️ Stopped.");
            }
            catch (OperationCanceledException)
            {
                AddLog("⏹️ Stopped (canceled).");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Stop error: {ex.Message}");
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _mainLoopTask = null;
                _isWatching = false;
            }
        }
        private async Task StartAllLoop()
        {
            try
            {

                // Nếu đang chạy, dừng trước
                if (_cts != null)
                {
                    AddLog("⚠️ Loops are already running. Restarting...");
                    await StopAllLoopAsync();
                }

                _cts = new CancellationTokenSource();
                _mainLoopTask = Task.Run(async () => await LoopAsync(_cts.Token));
            }
            catch (Exception ex)
            {
                AddLog($"❌ StartAllLoop error");
            }
        }
        private async Task StopAllLoopAsync()
        {
            if (_cts == null)
            {
                AddLog("ℹ️ No active loops to stop.");
                return;
            }

            AddLog("🛑 Stopping all loops...");
            btnStart.Text = "Stopping...";
            numInterval.Enabled = true;
            try
            {
                _cts.Cancel();

                // Đợi cho đến khi các vòng lặp dừng hẳn
                var tasks = new[] { _mainLoopTask }
                    .Where(t => t != null)
                    .ToArray()!;

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                AddLog($"⚠️ StopAllLoop error: {ex.Message}");
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _mainLoopTask = null;
                btnStart.Text = "Start";
                AddLog("✅ All loops stopped cleanly.");
            }
        }

        bool isKlasOpen = false;
        bool _attachedKlas = false, canConnect = false;
        private async Task LoopAsync(CancellationToken ct)
        {
            // Lấy interval hiện tại 1 lần khi start; nếu muốn thay đổi “nóng” hãy đọc lại mỗi vòng
            var ms = Math.Max(50, (int)numInterval.Value); // guard tối thiểu 50ms
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(ms));

            try
            {
                while (await timer.WaitForNextTickAsync(ct))
                {
                    await DoOneIterationAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // bình thường khi stop
            }
            catch (Exception ex)
            {
                // log rồi quyết định: tiếp tục hay thoát vòng lặp.
                // Ở đây mình chọn ghi log và tiếp tục (tuỳ nhu cầu có thể rethrow để dừng)
                AddLog($"⚠️ Loop error: {ex.Message}");
            }
        }
        private async Task DoOneIterationAsync(CancellationToken ct)
        {
            //_isWatching = true;
            int actionDelayMs = int.Parse(_Config.actionDelaySec) * 1000;
            int waitCmdQueryDelay = int.Parse(_Config.waitCmdQueryDelaySec) * 1000;
            int waitExcelDelayMs = int.Parse(_Config.waitExcelDelaySec) * 1000;
            int KlasScanIntervalSec = int.Parse(_Config.KlasScanIntervalSec) * 1000;

            int maxRetry = 5;
            int delayMs = 500;
            string dbMsg = string.Empty;
            string SfcMsg = string.Empty;

            //await _uiaWorker.Run(() =>
            //{
            //    rtxtLog.Clear();
            //});
            // 1. Open KLAS
            if (!isKlasOpen) isKlasOpen = OpenKLAS(); else AddLog("KLAS opened");
            await Task.Delay(actionDelayMs);

            // 2. Try to attach (OPEN KLAS, CONNECT DB, ATTACH PROCESS)
            if (!_attachedKlas)
            {
                AddLog("Start attach process");

                for (int attempt = 1; attempt <= maxRetry; attempt++)
                {
                    try
                    {
                        _automationService2.AttachToProcess(_Config.ProcessName);
                        _attachedKlas = true;
                        AddLog("Connected to KLAS successfully!");
                        _isWatching = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        AddLog($"============ Retry attach {attempt}/{maxRetry} ============");
                        //AddLog(ex.ToString());
                        await Task.Delay(delayMs);
                        continue;
                    }
                }

            }
            else
            {
                AddLog("Attached KLAS");
            }


            // 3. Log in
            if (!_isLogin)
            {
                // Điền acc/pass
                await _uiaWorker.Run(() =>
                {
                    _automationService2.SetText(_Config.Controls.Account, _Config.KlasAcc);
                    AddLog("Account");
                    _automationService2.SetText(_Config.Controls.Password, _Config.KlasPassword);
                    AddLog("Password");
                });
                await Task.Delay(actionDelayMs, ct);

                // click login btn
                await _uiaWorker.Run(() =>
                {
                    _automationService2.ClickTo(_Config.Controls.LoginBtn);
                    AddLog("LoginBtn");
                });
                await Task.Delay(actionDelayMs, ct);
                _isLogin = true;
            }
            else AddLog("Login done");

            // Step 5: refresh lại UI node cache do form thay đổi
            await _uiaWorker.Run(() => { _automationService2.AttachToProcess(_Config.ProcessName); AddLog("Reattached and refreshed main window."); });
            await Task.Delay(actionDelayMs, ct);

            // 4. Click label print log menu
            await _uiaWorker.Run(() => { _automationService2.ClickSubMenuById(_Config.Controls.PrintManagementMenu, _Config.Controls.LabelPrintLogMenu, 5000); AddLog("LabelPrintLogMenu"); });
            await Task.Delay(actionDelayMs, ct);



            // Step 5: refresh lại UI node cache do form thay đổi
            await _uiaWorker.Run(() => { _automationService2.AttachToProcess(_Config.ProcessName); AddLog("Reattached and refreshed main window."); });
            await Task.Delay(actionDelayMs, ct);


            var now = DateTime.Now.ToString("yyyy-MM-dd");
            var past = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            await _uiaWorker.Run(() => { _automationService2.SetText("dtpDate_S", "PART_TextBox", past); _automationService2.SetText("dtpDate_E", "PART_TextBox", now); AddLog("Input date range"); });
            await Task.Delay(actionDelayMs, ct);

            // Click searchBtn
            await _uiaWorker.Run(() => { _automationService2.ClickTo(_Config.Controls.SearchButton); AddLog("SearchButton"); });
            await Task.Delay(waitCmdQueryDelay, ct);


            // Step 6: Click cmdExcel (export to Excel)
            await _uiaWorker.Run(() => { _automationService2.ClickTo(_Config.Controls.ExcelButton); AddLog("ExcelButton"); });
            await Task.Delay(waitExcelDelayMs, ct);


            // Chuẩn bị file path
            var exportDir = _Config.saveFolder;
            if (!System.IO.Directory.Exists(exportDir))
                System.IO.Directory.CreateDirectory(exportDir);
            var fileName = System.IO.Path.Combine(exportDir, $"{DateTime.Now:yyyyMMddHHmmssff}.xlsx");



            // 7. Đợi Save dialog của app đích xuất hiện
            var res = await _automationService2.WaitTargetSaveDialogAsync(ct, TimeSpan.FromSeconds(8));

            if (res is { } found)
            {
                AddLog("found saveDialog");
                var (dlg, fileNameEdit, saveBtn) = found;

                // 3) Gõ đường dẫn file
                fileNameEdit.AsTextBox()?.Enter(fileName); AddLog("Edit file name");
                await Task.Delay(actionDelayMs, ct);
                // 4) Bấm Save
                // Ưu tiên pattern Invoke/SelectionItem/ExpandCollapse theo ClickTo của bạn, đơn giản hóa:
                saveBtn.AsButton()?.Invoke(); AddLog("Click save");
                await Task.Delay(actionDelayMs, ct);

                var okClicked = await _automationService2.WaitNextOkDialogAsync(ct, TimeSpan.FromSeconds(10), null); AddLog("Click OK after save");
                await Task.Delay(actionDelayMs, ct);



                if (okClicked)
                {
                    AddLog("Saved excel");

                    // ========== 1) Đọc Excel ==========
                    AddLog("📖 Start reading file...");
                    List<Dictionary<string, string>> kvRows;
                    try
                    {
                        kvRows = ReadExcelRows(fileName);
                        if (kvRows == null || kvRows.Count == 0) { AddLog("⚠️ File trống hoặc không có dữ liệu. Bỏ qua upload."); return; }
                    }
                    catch (Exception ex) { AddLog($"❌ Lỗi đọc Excel: {ex.Message}"); return; }

                    // Lọc trùng WO cho dtos
                    var dtos = MapRowsToDtos(kvRows).Where(d => !string.IsNullOrWhiteSpace(d.Wo) && d.Wo.Length == 13 && d.StartSn.Contains("HM", StringComparison.OrdinalIgnoreCase)).ToList();

                    if (dtos.Count == 0) return;

                    try
                    {
                        var count = await UploadDtosToDbAsync(dtos, () => _dbFactory.CreateDbContextAsync(), ct); AddLog($"✅ Imported {count} rows from {Path.GetFileName(fileName)}");
                    }
                    catch (Exception ex) { AddLog($"❌ Lỗi upload DB: {ex.Message}"); return; }

                    File.Delete(fileName);
                    // ========== 4) Nghỉ nhịp ==========
                    AddLog($"⏱️ Wait for next upload after {_interval} sec");
                    await Task.Delay(_interval * 1000, ct);
                }
                else
                {
                    AddLog("Not click OK after save file");
                }

            }
            else
            {
                AddLog("Not found saveDialog");
            }
            // ví dụ demo:
            await Task.Yield();
            // ... công việc ...
        }
        private async Task<bool> IsLoggedInAsync(CancellationToken ct)
        {
            return await _uiaWorker.Run(() =>
            {
                // Cách 1: thấy nút Logout
                var el = _automationService2.GetControlById(_Config.Controls.LogoutBtn);
                if (el != null && el.Properties.IsEnabled.ValueOrDefault) return true;

                // Cách 2: thấy menu cha
                var menu = _automationService2.GetControlById(_Config.Controls.PrintManagementMenu);
                if (menu != null && !menu.BoundingRectangle.IsEmpty) return true;

                return false;
            });
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
        private async void frmKLAS_FormClosing(object sender, FormClosingEventArgs e)
        {
            //await StopRetriveDataFromKLAS();

            //base.OnFormClosing(e);
        }

        #region Đọc File Excel của KLAS
        public async Task<bool> WaitForFileStableAsync(string path, TimeSpan timeout, CancellationToken ct)
        {
            var deadline = DateTime.UtcNow + timeout;
            long lastLen = -1;
            int stableCount = 0;

            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        stableCount = 0;
                    }
                    else
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var len = fs.Length;
                            if (len > 0 && len == lastLen)
                            {
                                stableCount++;
                                if (stableCount >= 3) return true; // ổn định 3 nhịp
                            }
                            else
                            {
                                stableCount = 0;
                                lastLen = len;
                            }
                        }
                    }
                }
                catch
                {
                    stableCount = 0; // vẫn đang bị khóa/đang ghi
                }

                await Task.Delay(150, ct).ConfigureAwait(false);
            }
            return false;
        }

        // Đọc Excel về các dòng key-value (map theo header linh hoạt)
        public List<Dictionary<string, string>> ReadExcelRows(string xlsxPath)
        {
            var rows = new List<Dictionary<string, string>>();
            using var wb = new ClosedXML.Excel.XLWorkbook(xlsxPath);
            var ws = wb.Worksheets.FirstOrDefault() ?? throw new InvalidOperationException("Workbook không có worksheet.");

            var used = ws.RangeUsed() ?? throw new InvalidOperationException("Worksheet trống.");
            var firstRow = used.RangeAddress.FirstAddress.RowNumber;
            var lastRow = used.RangeAddress.LastAddress.RowNumber;
            var firstCol = used.RangeAddress.FirstAddress.ColumnNumber;
            var lastCol = used.RangeAddress.LastAddress.ColumnNumber;

            // Header
            var headers = new List<string>();
            for (int c = firstCol; c <= lastCol; c++)
            {
                var h = ws.Cell(firstRow, c).GetString()?.Trim() ?? "";
                headers.Add((h));
            }

            // Data
            for (int r = firstRow + 1; r <= lastRow; r++)
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                bool allEmpty = true;

                for (int c = firstCol; c <= lastCol; c++)
                {
                    var raw = ws.Cell(r, c).GetFormattedString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(raw)) allEmpty = false;
                    dict[headers[c - firstCol]] = raw;
                }

                if (!allEmpty) rows.Add(dict);
            }
            return rows;
        }

        // Helper lấy giá trị theo nhiều alias header
        private static string Pick(Dictionary<string, string> row, params string[] aliases)
        {
            foreach (var a in aliases)
            {
                if (row.TryGetValue(a, out var v) && !string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }
            return "";
        }

        // Map 1 dòng excel -> DTO/entity tạm
        public class ScanRowDto
        {
            public string PID { get; set; }
            public string EBR { get; set; }
            public string WorkOrder { get; set; }
            public int Qty { get; set; } = 1;
            public DateTime? ScanAt { get; set; }
        }

        public List<TbKla> MapRowsToDtos(List<Dictionary<string, string>> rows)
        {
            var list = new List<TbKla>();
            foreach (var r in rows)
            {
                var wo = Pick(r, "Work Order");
                var ebr = Pick(r, "Model.Suffix");
                var startSN = Pick(r, "Start S/N");
                var endSN = Pick(r, "End S/N");
                var startSerial = Pick(r, "Start Serial");
                var endSerial = Pick(r, "End Serial");



                if (!string.IsNullOrWhiteSpace(wo))
                {
                    list.Add(new TbKla
                    {
                        Wo = wo,
                        Ebr = ebr,
                        StartSn = startSN,
                        EndSn = endSN,
                        StartSerial = startSerial,
                        EndSerial = endSerial,
                    });
                }
            }
            return list;
        }

        // Đẩy DB: tùy schema của bạn. Ví dụ với TbScanOut (Pomelo MySQL).
        public async Task<int> UploadDtosToDbAsync(IEnumerable<TbKla> items, Func<Task<MMesDbContext>> dbFactory, CancellationToken ct)
        {
            int affected = 0;
            await using var db = await dbFactory();

            foreach (var it in items)
            {
                ct.ThrowIfCancellationRequested();

                // Check if Wo exists
                var entity = await db.TbKlas.FirstOrDefaultAsync(x => x.Wo == it.Wo, ct);
                if (entity != null)
                {
                    // Update existing
                    entity.Ebr = it.Ebr;
                    entity.StartSn = it.StartSn;
                    entity.EndSn = it.EndSn;
                    entity.StartSerial = it.StartSerial;
                    entity.EndSerial = it.EndSerial;
                }
                else
                {
                    // Insert new
                    entity = new TbKla
                    {
                        Wo = it.Wo,
                        Ebr = it.Ebr,
                        StartSn = it.StartSn,
                        EndSn = it.EndSn,
                        StartSerial = it.StartSerial,
                        EndSerial = it.EndSerial,
                    };
                    db.TbKlas.Add(entity);
                }
                affected++;
            }

            await db.SaveChangesAsync(ct);
            return affected;
        }
        #endregion

        private void frmKLAS_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide(); // ẩn cửa sổ
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(
                    1000,
                    "KLAS Clone Data Is Running",
                    "Double click to open app.",
                    ToolTipIcon.Info
                );
            }
        }

        private void numInterval_ValueChanged(object sender, EventArgs e)
        {
            _interval = (int)numInterval.Value;
        }

        private void frmKLAS_Load(object sender, EventArgs e)
        {
            _interval = (int)numInterval.Value;
        }
    }


    public static class RectangleExtensions
    {
        public static double Area(this Rectangle r)
        {
            return r.Width * r.Height;
        }
    }
}
