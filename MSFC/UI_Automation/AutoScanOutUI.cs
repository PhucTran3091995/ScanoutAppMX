using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3.Patterns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.Logging;
using MSFC.Models;
using MSFC.UI_Automation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UIAutoLib.Services;
using static ZXing.QrCode.Internal.Version;
using Timer = System.Timers.Timer;


namespace ScanOutTool.Services
{
    public interface IAutoScanOutUI
    {
        /// <summary>
        /// Bắn ra khi textbox PID trong SFC thay đổi.
        /// </summary>
        public event Action<string> PIDChanged;

        public string ReadMainWindowTitle();
        public string ReadPID();
        public string ReadEBR();
        public string ReadWO();
        public string ReadResult();
        public string ReadMessage();
        public string ReadProgress();

        // NEW – click đơn giản
        public string ClickClear(); // lấy từ config hoặc mặc định "btnClear"


        /// <summary>
        /// Lấy vị trí/kích thước phần tử kết quả trong màn hình SFC.
        /// </summary>
        public (double X, double Y, double Width, double Height) GetResultElementBounds();

        /// <summary>
        /// Handle cửa sổ chính của SFC.
        /// </summary>
        public IntPtr GetMainHandle();

        /// <summary>
        /// Chờ UI thay đổi rồi đọc đầy đủ thông tin PCB.
        /// Lưu ý: nếu bạn đang return null khi không có thay đổi,
        /// và dự án bật nullable, hãy đổi thành Task&lt;PCB?&gt;.
        /// </summary>
        public Task<PCB> ReadPCBInfor();

        /// <summary>
        /// Kiểm tra đúng màn ScanOut (ví dụ bằng PROCID).
        /// </summary>
        public bool IsScanoutUI();
    }

    public interface ILoggingService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
       
    }

    public sealed class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message) => _logger.LogInformation("{Message}", message);
        public void LogWarning(string message) => _logger.LogWarning("{Message}", message);
        public void LogError(string message) => _logger.LogError("{Message}", message);

        public void LogError(Exception exception, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                _logger.LogError(exception, "Unhandled exception");
            else
                _logger.LogError(exception, "{Message}", message);
        }
    }

    public class AutoScanOutUI : IAutoScanOutUI, IDisposable
    {
        private UiAutomationSettings _cfg;
        private readonly ILoggingService _loggingService;

        private readonly AutomationService automation;
        private bool isAttached = false;

        // cache
        private string oldProgress = "";
        private string oldMessage = "";
        private string oldResult = "";
        private string oldPID = "";

        // events/signal
        public event Action<string>? PIDChanged;
        private readonly object _tcsLock = new();
        private TaskCompletionSource<bool>? _changeTcs;

        // timers/loops
        private readonly System.Timers.Timer _watchdogTimer; // re-attach watcher
        private CancellationTokenSource? _pollCts;
        private Task? _pollTask;

        // dùng OptionsMonitor để cho phép reload nóng
        public AutoScanOutUI(IOptionsMonitor<UiAutomationSettings> options, ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _cfg = options.CurrentValue;

            // init automation
            automation = new AutomationService();

            // attach theo ProcessName từ config
            isAttached = automation.AttachToProcess(_cfg.ProcessName);
            if (!isAttached)
                throw new Exception($"Failed to attach to the process. Please open App '{_cfg.ProcessName}'");

            // subscribe thay đổi config
            options.OnChange(newCfg =>
            {
                _loggingService?.LogInformation("UiAutomation settings changed, applying...");
                var processChanged = !string.Equals(_cfg.ProcessName, newCfg.ProcessName, StringComparison.Ordinal);
                _cfg = newCfg;

                if (processChanged)
                {
                    // reattach nếu đổi process
                    isAttached = automation.AttachToProcess(_cfg.ProcessName);
                    _loggingService?.LogInformation(isAttached
                        ? $"Re-attached to process '{_cfg.ProcessName}' (id {automation.ProcessId})"
                        : $"Re-attach failed for '{_cfg.ProcessName}'");
                }

                // cập nhật watchdog interval nếu cần
                if (_watchdogTimer != null)
                {
                    _watchdogTimer.Interval = Math.Max(50, _cfg.Timings.WatchdogIntervalMs);
                }
            });

            // watchdog
            _watchdogTimer = new System.Timers.Timer(Math.Max(50, _cfg.Timings.WatchdogIntervalMs))
            {
                AutoReset = true,
                Enabled = true
            };
            _watchdogTimer.Elapsed += WatchdogElapsed;

            // vòng polling chính
            _pollCts = new CancellationTokenSource();
            _pollTask = Task.Run(() => PollLoopAsync(_pollCts.Token));
        }

        // ====== IAutoScanOutUI ======
        public string ReadMainWindowTitle()
            => EnsureAttached() ? automation.GetMainWindowTitle() : throw new Exception("Not attached.");

        public string ReadPID() => EnsureAttached() ? RawRead(_cfg.ControlIds.PidTextBox) : throw new Exception("Not attached.");
        public string ReadEBR() => EnsureAttached() ? RawRead(_cfg.ControlIds.EbrTextBox).TrimEnd('.') : throw new Exception("Not attached.");
        public string ReadWO() => EnsureAttached() ? RawRead(_cfg.ControlIds.WoTextBox) : throw new Exception("Not attached.");
        public string ReadResult() => EnsureAttached() ? RawRead(_cfg.ControlIds.ResultTextBox) : throw new Exception("Not attached.");
        public string ReadMessage() => EnsureAttached() ? RawRead(_cfg.ControlIds.MessageTextBox) : throw new Exception("Not attached.");
        public string ReadProgress() => EnsureAttached() ? RawRead(_cfg.ControlIds.ProgressText) : throw new Exception("Not attached.");

        public string ClickClear() => EnsureAttached() ? RawClick(_cfg.ControlIds.ButtonText) : throw new Exception("Not attached.");

        public (double X, double Y, double Width, double Height) GetResultElementBounds()
        {
            if (!EnsureAttached()) throw new Exception("Not attached.");
            var b = automation.GetElementBounds(_cfg.ControlIds.ResultTextBox);
            return (b.X, b.Y, b.Width, b.Height);
        }

        public IntPtr GetMainHandle()
            => EnsureAttached() ? automation.GetMainHandle() : throw new Exception("Not attached.");

        public bool IsScanoutUI()
        {
            if (!EnsureAttached()) return false;
            var proc = RawRead(_cfg.ControlIds.ProcessIdText);
            var token = _cfg.ProcIdContains;
            if (string.IsNullOrWhiteSpace(token)) return true; // nếu không cấu hình, coi như pass
            return proc?.Contains(token, StringComparison.OrdinalIgnoreCase) == true;
        }

        public async Task<PCB> ReadPCBInfor()
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_tcsLock) _changeTcs = tcs;

            var timeoutTask = Task.Delay(Math.Max(100, _cfg.Timings.ReadTimeoutMs));
            var signaled = await Task.WhenAny(tcs.Task, timeoutTask) == tcs.Task;

            return new PCB
            {
                PID = ReadPID(),
                EBR = ReadEBR(),
                WO = ReadWO(),
                Result = ReadResult(),
                Message = ReadMessage(),
                Progress = ReadProgress(),
            };
        }

        // ====== Polling core ======
        private async Task PollLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!EnsureAttached())
                    {
                        await Task.Delay(500, token);
                        continue;
                    }

                    var ids = _cfg.ControlIds;
                    var curPID = RawRead(ids.PidTextBox);
                    var curResult = RawRead(ids.ResultTextBox);
                    var curMessage = RawRead(ids.MessageTextBox);
                    var curProg = RawRead(ids.ProgressText);
                    //var curBtn = RawRead(ids.ButtonText);

                    bool changed = false;

                    if (!StringEquals(curPID, oldPID))
                    {
                        oldPID = curPID;
                        changed = true;

                        var debounce = Math.Max(0, _cfg.Timings.EventDebounceMs);
                        if (debounce > 0) await Task.Delay(debounce, token);
                        PIDChanged?.Invoke(curPID ?? string.Empty);
                    }

                    if (!StringEquals(curResult, oldResult)) { oldResult = curResult; changed = true; }
                    if (!StringEquals(curMessage, oldMessage)) { oldMessage = curMessage; changed = true; }
                    if (!StringEquals(curProg, oldProgress)) { oldProgress = curProg; changed = true; }

                    if (changed)
                    {
                        TaskCompletionSource<bool>? tcs;
                        lock (_tcsLock) tcs = _changeTcs;
                        tcs?.TrySetResult(true);
                    }

                    await Task.Delay(Math.Max(20, _cfg.Timings.PollIntervalMs), token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _loggingService?.LogError($"PollLoop error: {ex.Message}");
                    await Task.Delay(200, token);
                }
            }
        }

        // ====== Watchdog re-attach ======
        private void WatchdogElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                var currentPid = GetProcessIdByName(_cfg.ProcessName);
                if (currentPid != automation.ProcessId)
                {
                    _loggingService?.LogInformation($"Process '{_cfg.ProcessName}' changed from {automation.ProcessId} to {currentPid}");
                    isAttached = automation.AttachToProcess(_cfg.ProcessName);
                    if (!isAttached)
                        _loggingService?.LogError($"Cannot attach to '{_cfg.ProcessName}' (id {automation.ProcessId})");
                    else
                        _loggingService?.LogInformation($"Attached to '{_cfg.ProcessName}' (id {automation.ProcessId})");
                }
            }
            catch (Exception ex)
            {
                _loggingService?.LogError($"Watchdog error: {ex.Message}");
            }
        }

        // ====== Helpers ======
        private bool EnsureAttached() => isAttached;

        private string RawRead(string automationIdOrName)
            => automation.ReadTextByAutomationId(automationIdOrName);

        private string RawClick(string automationIdOrName)
        {
            bool result = automation.ClickByAutomationId(automationIdOrName);

            if (result)
            {
               return $"Clicked control '{automationIdOrName}' thành công.";
            }
            else
            {
                return $"Không tìm thấy hoặc không click được control '{automationIdOrName}'.";
            }
        }

        private int? GetProcessIdByName(string name)
            => Process.GetProcessesByName(name).FirstOrDefault()?.Id;

        private static bool StringEquals(string? a, string? b)
            => string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.Ordinal);

        public void Dispose()
        {
            try { _watchdogTimer?.Stop(); _watchdogTimer?.Dispose(); } catch { }
            try
            {
                _pollCts?.Cancel();
                _pollTask?.Wait(500);
                _pollCts?.Dispose();
            }
            catch { }
        }
    }

}
