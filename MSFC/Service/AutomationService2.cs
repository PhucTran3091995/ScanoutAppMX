
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Application = FlaUI.Core.Application;

namespace MSFC.Service
{
    /// <summary>
    /// Interface định nghĩa các chức năng cơ bản của AutomationService2
    /// (giữ nguyên chữ ký cũ để không phá tương thích)
    /// </summary>
    public interface IAutomationService2 : IDisposable
    {

        void AttachToProcess(int processId);
        void AttachToProcess(string processName);

        IReadOnlyList<AutomationElement> ScanApp();

        AutomationElement GetControlById(string automationId);
        AutomationElement GetChildControl(string parentAutomationId, string childAutomationId);
        AutomationElement GetChildControl(string parentAutomationId, string childAutomationId, int index);

        void ClickTo(string automationId);
        void SetText(string automationId, string text);
        void SetText(string parentAutomationId, string childAutomationId, string text, int index = 0);

        string GetText(string automationId);
        string GetText(string parentAutomationId, string childAutomationId, int index = 0); // overload

        string SafeGetText(AutomationElement el);
        string GetInnerMessageFromResult(string resultUcId, string resultTextId, bool pickLastNonEmpty = true);
       

        // Tiện ích
        void RefreshCache();
        IReadOnlyList<AutomationElement> GetControlsById(string automationId);
        AutomationElement GetNthById(string automationId, int index);
        AutomationElement GetControlByPath(params string[] automationIds);

        //public Task<AutomationElement?> WaitWindowByNameOrClassAsync(CancellationToken ct, TimeSpan timeout, params string[] namesOrClasses);
        //public AutomationElement? FindFileNameEdit(AutomationElement dialog);
        //public void FocusAndPaste(AutomationElement el, string text);

        //public Task<(AutomationElement dialog, AutomationElement fileNameEdit)?> WaitCommonFileDialogAsync(CancellationToken ct, TimeSpan timeout);

        //public Task<bool> WaitSuccessDialogAsync(CancellationToken ct, TimeSpan timeout);
        //AutomationElement? FindButtonByNamesOrIds(AutomationElement scope, params string[] namesOrIds);

        public Task<(AutomationElement dialog, AutomationElement fileNameEdit, AutomationElement saveButton)?> WaitTargetSaveDialogAsync(CancellationToken ct, TimeSpan timeout);
        public Task<bool> WaitNextOkDialogAsync(CancellationToken ct, TimeSpan timeout, IntPtr? lastClosedDialogHwnd = null);

        public bool ClickSubMenuById(string parentId, string childId, int timeoutMs = 2000);

    }

    /// <summary>
    /// Triển khai IAutomationService2 sử dụng FlaUI (UIA3)
    /// Phiên bản tương thích rộng (FlaUI 4.x/5.x):
    /// - Không dùng CacheRequest / Cached API để tránh khác biệt giữa các version
    /// - Đã tối ưu hot-path: tránh FindAllDescendants khi không cần, ưu tiên FindFirst
    /// - SetValue (ValuePattern) thay vì Enter() khi có thể
    /// - Cache map AutomationId -> elements để lookup nhanh
    /// </summary>
    public class AutomationService2 : IAutomationService2
    {

        private UIA3Automation _automation;
        private Application _app;
        private Window _mainWindow;
        private int _processId;

        //private IReadOnlyList<AutomationElement> _cachedNodes = Array.Empty<AutomationElement>();
        private List<AutomationElement> _cachedNodes = new();

        private Dictionary<string, List<AutomationElement>> _elementsById =
            new Dictionary<string, List<AutomationElement>>(StringComparer.Ordinal);

        // ===================== Attach / Scan / Cache =====================


        public void AttachToProcess(int processId)
        {
            DisposeInternal();

            _automation = new UIA3Automation();
            _app = Application.Attach(processId);
            _mainWindow = _app.GetMainWindow(_automation)
                ?? throw new InvalidOperationException("Cannot get main window of process.");
            _processId = processId;

            CacheAllNodes();
        }

        public void AttachToProcess(string processName)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault()
                ?? throw new InvalidOperationException($"Process '{processName}' not found.");
            AttachToProcess(process.Id);
        }

        public void RefreshCache() => CacheAllNodes();

        private void CacheAllNodes()
        {
            if (_mainWindow == null)
            {
                //_cachedNodes = Array.Empty<AutomationElement>();
                _cachedNodes = new List<AutomationElement>();
                _elementsById = new(StringComparer.Ordinal);
                return;
            }

            //// Quét 1 lần toàn cây; tránh lặp lại trong hot-path
            //_cachedNodes = _mainWindow.FindAllDescendants();
            _cachedNodes.AddRange(_mainWindow.FindAllDescendants());

            _elementsById = _cachedNodes
                .Where(e => !string.IsNullOrWhiteSpace(e.AutomationId))
                .GroupBy(e => e.AutomationId, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        }

        // Trong AutomationService2
        public bool RefreshMainWindowAndCache(int timeoutMs = 5000, Func<AutomationElement, bool>? ready = null)
        {
            if (_app == null || _automation == null) return false;

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            var oldHwnd = _mainWindow?.Properties.NativeWindowHandle.ValueOrDefault ?? 0;

            while (DateTime.UtcNow < deadline)
            {
                var newMain = _app.GetMainWindow(_automation);
                if (newMain != null)
                {
                    var newHwnd = newMain.Properties.NativeWindowHandle.ValueOrDefault;

                    // Điều kiện đổi window: handle khác HOẶC predicate 'ready' đạt
                    var ok = (newHwnd != 0 && newHwnd != oldHwnd)
                             || (ready != null && ready(newMain));
                    if (ok)
                    {
                        // clear cache cũ, bind lại root và cache mới
                        DisposeCacheOnly();
                        _mainWindow = newMain;
                        CacheAllNodes();
                        return true;
                    }
                }

                System.Threading.Thread.Sleep(100);
            }

            return false;
        }

        // Chỉ dọn cache map, KHÔNG dispose automation/app
        private void DisposeCacheOnly()
        {
            try { _cachedNodes?.Clear(); } catch { }
        }


        public IReadOnlyList<AutomationElement> ScanApp()
            => _mainWindow == null ? Array.Empty<AutomationElement>() : _mainWindow.FindAllDescendants();

        // ===================== Find controls =====================

        public AutomationElement GetControlById(string automationId)
        {
            if (string.IsNullOrWhiteSpace(automationId)) return null;
            return _elementsById.TryGetValue(automationId, out var list) ? list.FirstOrDefault() : null;
        }

        public IReadOnlyList<AutomationElement> GetControlsById(string automationId)
        {
            if (string.IsNullOrWhiteSpace(automationId)) return Array.Empty<AutomationElement>();
            return _elementsById.TryGetValue(automationId, out var list) ? list : Array.Empty<AutomationElement>();
        }

        public AutomationElement GetNthById(string automationId, int index)
        {
            var list = GetControlsById(automationId);
            return (index >= 0 && index < list.Count) ? list[index] : null;
        }

        public AutomationElement GetChildControl(string parentAutomationId, string childAutomationId)
            => GetChildControl(parentAutomationId, childAutomationId, 0);

        public AutomationElement GetChildControl(string parentAutomationId, string childAutomationId, int index)
        {
            if (string.IsNullOrWhiteSpace(parentAutomationId) || string.IsNullOrWhiteSpace(childAutomationId))
                return null;

            var parents = GetControlsById(parentAutomationId);
            if (parents.Count == 0) return null;

            // Tối ưu: thử FindFirst trước; chỉ khi cần index > 0 mới FindAll
            foreach (var p in parents)
            {
                var first = p.FindFirstDescendant(cf => cf.ByAutomationId(childAutomationId));
                if (first != null && index == 0) return first;

                if (index > 0)
                {
                    var all = p.FindAllDescendants(cf => cf.ByAutomationId(childAutomationId));
                    if (all != null && all.Length > index)
                        return all[index];
                }
            }
            return null;
        }

        public AutomationElement GetControlByPath(params string[] automationIds)
        {
            if (automationIds == null || automationIds.Length == 0) return null;

            var current = GetControlsById(automationIds[0]);
            if (current.Count == 0) return null;

            for (int i = 1; i < automationIds.Length; i++)
            {
                string id = automationIds[i];
                AutomationElement next = null;

                // Dừng ngay khi thấy match đầu tiên để giảm chi phí
                for (int p = 0; p < current.Count && next == null; p++)
                    next = current[p].FindFirstDescendant(cf => cf.ByAutomationId(id));

                if (next == null) return null;

                current = new List<AutomationElement>(1) { next };
            }

            return current[0];
        }

        #region Click To
        public void ClickTo(string automationId)
        {
            var control = GetControlById(automationId);
            if (control == null) return;
            ClickElement(control);
        }

        public void ClickElement(AutomationElement control)
        {
            try
            {
                // Luôn cố focus trước
                TryFocus(control);

                var ctrlType = control.Properties.ControlType.Value;

                // 1) WPF MenuItem xử lý riêng
                if (ctrlType == FlaUI.Core.Definitions.ControlType.MenuItem)
                {
                    if (TryClickMenuItem(control)) return;
                    // Nếu chưa return: fallback click
                    FallbackClick(control);
                    return;
                }

                // 2) InvokePattern (nút, item không có submenu)
                var inv = control.Patterns.Invoke;
                if (inv.IsSupported)
                {
                    inv.Pattern.Invoke();
                    return;
                }

                // 3) Expand/Collapse (menu cha, combobox item có submenu)
                var expand = control.Patterns.ExpandCollapse;
                if (expand.IsSupported)
                {
                    if (expand.Pattern.ExpandCollapseState == FlaUI.Core.Definitions.ExpandCollapseState.Collapsed)
                        expand.Pattern.Expand();
                    else
                        expand.Pattern.Collapse();
                    return;
                }

                // 4) SelectionItem (một số menu/list hỗ trợ chọn)
                var select = control.Patterns.SelectionItem;
                if (select.IsSupported)
                {
                    select.Pattern.Select();
                    return;
                }

                // 5) Button “thật” (FlaUI wrapper)
                var btn = control.AsButton();
                if (btn != null)
                {
                    btn.Invoke();
                    return;
                }

                // 6) Image / Pane / Hyperlink / Custom → click tọa độ
                if (ctrlType == FlaUI.Core.Definitions.ControlType.Image ||
                    ctrlType == FlaUI.Core.Definitions.ControlType.Pane ||
                    ctrlType == FlaUI.Core.Definitions.ControlType.Hyperlink ||
                    ctrlType == FlaUI.Core.Definitions.ControlType.Custom)
                {
                    FallbackClick(control);
                    return;
                }

                // 7) Fallback cuối
                FallbackClick(control);
            }
            catch
            {
                // nuốt lỗi để không ngắt luồng, bạn có thể AddLog nếu muốn
            }
        }

        private bool TryClickMenuItem(AutomationElement mi)
        {
            // 1) Nếu có Invoke → gọi luôn (thường là leaf)
            var inv = mi.Patterns.Invoke;
            if (inv.IsSupported)
            {
                inv.Pattern.Invoke();
                return true;
            }

            // 2) Nếu là leaf nhưng không có Invoke → thử LegacyIAccessible
            var legacy = mi.Patterns.LegacyIAccessible;
            if (legacy.IsSupported)
            {
                try
                {
                    legacy.Pattern.DoDefaultAction();
                    return true;
                }
                catch { /* ignore */ }
            }

            // 3) Nếu có SelectionItem → Select (nhiều menu expose kiểu này)
            var select = mi.Patterns.SelectionItem;
            if (select.IsSupported)
            {
                select.Pattern.Select();
                return true;
            }

            // 4) Nếu có submenu → Expand (Menu cha)
            var expand = mi.Patterns.ExpandCollapse;
            if (expand.IsSupported)
            {
                if (expand.Pattern.ExpandCollapseState == FlaUI.Core.Definitions.ExpandCollapseState.Collapsed)
                    expand.Pattern.Expand();
                else
                    expand.Pattern.Collapse();
                return true;
            }

            // 5) Cuối cùng: click tọa độ (nhiều WPF menu vẫn ăn được)
            FallbackClick(mi);
            return true;
        }

        private void FallbackClick(AutomationElement el)
        {
            var br = el.BoundingRectangle;
            if (!br.IsEmpty)
            {
                FlaUI.Core.Input.Mouse.MoveTo(br.Center());
                FlaUI.Core.Input.Mouse.Click();
            }
        }

        private void TryFocus(AutomationElement el)
        {
            try
            {
                if (!el.Properties.HasKeyboardFocus.ValueOrDefault)
                {
                    el.Focus();
                }
            }
            catch { /* ignore */ }
        }

        // ⚙️ gọi: ClickSubMenuById("KALP0000", "KALP0070");
        public bool ClickSubMenuById(string parentId, string childId, int timeoutMs = 2000)
        {
            var parent = GetControlById(parentId);
            if (parent == null) return false;

            // 1) Mở menu cha (Expand hoặc hover nếu không có pattern)
            var expand = parent.Patterns.ExpandCollapse;
            if (expand.IsSupported &&
                expand.Pattern.ExpandCollapseState == FlaUI.Core.Definitions.ExpandCollapseState.Collapsed)
            {
                TryFocus(parent);
                expand.Pattern.Expand();
            }
            else
            {
                var br = parent.BoundingRectangle;
                if (!br.IsEmpty)
                {
                    FlaUI.Core.Input.Mouse.MoveTo(br.Center());
                    System.Threading.Thread.Sleep(500);   // cho popup kịp hiện
                                                          // FlaUI.Core.Input.Mouse.Click();   // bật nếu theme cần click để mở
                }
            }

            // 2) Đợi và tìm item con từ Desktop (popup là window riêng)
            var child = WaitMenuItemVisible(childId, timeoutMs);
            if (child == null)
                return false;

            // 3) Click menu con
            return TryClickMenuItem(child);
        }

        private AutomationElement? WaitMenuItemVisible(string idOrName, int timeoutMs)
        {
            var desktop = _automation.GetDesktop();
            var cf = _automation.ConditionFactory;
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

            // Nếu có PID app đích, thêm filter để không ăn nhầm app khác
            // var pidCond = cf.ByProcessId(_attachedPid);

            while (DateTime.UtcNow < deadline)
            {
                // Ưu tiên tìm theo AutomationId
                var el = desktop.FindFirstDescendant(
                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem)
                      .And(cf.ByAutomationId(idOrName))
                      .And(new PropertyCondition(_automation.PropertyLibrary.Element.IsOffscreen, false))
                // .And(pidCond)
                );
                if (el != null) return el;

                // fallback theo Name
                el = desktop.FindFirstDescendant(
                    cf.ByControlType(FlaUI.Core.Definitions.ControlType.MenuItem)
                      .And(cf.ByName(idOrName))
                      .And(new PropertyCondition(_automation.PropertyLibrary.Element.IsOffscreen, false))
                // .And(pidCond)
                );
                if (el != null) return el;

                System.Threading.Thread.Sleep(500);
            }
            return null;
        }
        #endregion



        public void SetText(string automationId, string text)
        {
            var el = GetControlById(automationId);
            SetTextCore(el, text);
        }

        public void SetText(string parentAutomationId, string childAutomationId, string text, int index = 0)
        {
            var el = GetChildControl(parentAutomationId, childAutomationId, index);
            SetTextCore(el, text);
        }

        private static void SetTextCore(AutomationElement el, string text)
        {
            if (el == null) return;
            try
            {
                var vp = el.Patterns.Value;
                if (vp.IsSupported)
                {
                    vp.Pattern.SetValue(text ?? string.Empty);
                    return;
                }
                el.AsTextBox()?.Enter(text ?? string.Empty); // fallback
            }
            catch
            {
                // ignore
            }
        }


        public string GetText(string automationId)
        {
            var control = GetControlById(automationId);
            return SafeGetText(control);
        }

        public string GetText(string parentAutomationId, string childAutomationId, int index = 0)
        {
            var el = GetChildControl(parentAutomationId, childAutomationId, index);
            return SafeGetText(el);
        }

        /// <summary>
        /// Đọc text linh hoạt theo ControlType/Pattern:
        /// - Edit/TextBox: ValuePattern.Value
        /// - Text/TextBlock/Label: Name
        /// - Document/RichText: TextPattern
        /// - LegacyIAccessible: Value/Name
        /// - Fallback: Name hoặc tìm Text child
        /// </summary>
        public string SafeGetText(AutomationElement el)
        {
            if (el == null) return null;

            try
            {
                // TextBox/Edit & control có ValuePattern
                var vp = el.Patterns.Value;
                if (vp.IsSupported)
                {
                    var v = vp.Pattern.Value;
                    if (!string.IsNullOrEmpty(v)) return v;
                }

                // Document / RichText
                var tp = el.Patterns.Text;
                if (tp.IsSupported)
                {
                    var range = tp.Pattern.DocumentRange;
                    var t = range?.GetText(int.MaxValue);
                    if (!string.IsNullOrEmpty(t)) return t.TrimEnd('\r', '\n');
                }

                // WPF TextBlock / Label
                if (el.ControlType == ControlType.Text && !string.IsNullOrEmpty(el.Name))
                    return el.Name;

                // LegacyIAccessible
                var lp = el.Patterns.LegacyIAccessible;
                if (lp.IsSupported)
                {
                    var v = lp.Pattern.Value;
                    if (!string.IsNullOrEmpty(v)) return v;
                    if (!string.IsNullOrEmpty(lp.Pattern.Name)) return lp.Pattern.Name;
                }

                // Fallback Name
                if (!string.IsNullOrEmpty(el.Name)) return el.Name;

                // Tìm text con
                var txt = el.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text));
                if (txt != null && !string.IsNullOrEmpty(txt.Name)) return txt.Name;

                return string.Empty;
            }
            catch
            {
                return null;
            }
        }

        public string GetInnerMessageFromResult(string resultUcId, string resultTextId, bool pickLastNonEmpty = true)
        {
            var uc = GetControlById(resultUcId);
            if (uc == null) return null;

            var texts = uc.FindAllDescendants(cf => cf.ByControlType(ControlType.Text));
            if (texts == null || texts.Length == 0) return null;

            if (pickLastNonEmpty)
            {
                for (int i = texts.Length - 1; i >= 0; i--)
                {
                    var t = texts[i];
                    if (string.Equals(t.AutomationId, resultTextId, StringComparison.Ordinal)) continue;
                    var name = t.Name;
                    if (!string.IsNullOrWhiteSpace(name)) return name.Trim();
                }
            }
            else
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    var t = texts[i];
                    if (string.Equals(t.AutomationId, resultTextId, StringComparison.Ordinal)) continue;
                    var name = t.Name;
                    if (!string.IsNullOrWhiteSpace(name)) return name.Trim();
                }
            }

            return null;
        }
        //public string GetInnerMessageFromResult(string resultUcId, string resultTextId, bool pickLastNonEmpty = true)
        //{
        //    var uc = GetControlById(resultUcId);
        //    if (uc == null) return null;

        //    // Tất cả TextBlock (ControlType.Text) bên trong ucResult
        //    var texts = uc.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text));
        //    if (texts == null || texts.Length == 0) return null;

        //    // Lọc: bỏ chính txtResult (cái hiển thị OK/NG) và bỏ rỗng
        //    var filtered = texts
        //        .Where(t => !string.Equals(t.AutomationId, resultTextId, StringComparison.Ordinal))
        //        .Where(t => !string.IsNullOrWhiteSpace(t.Name))
        //        .ToList();

        //    if (filtered.Count == 0) return null;

        //    // Thường message là TextBlock “sâu/đằng sau” → chọn cái cuối
        //    var chosen = pickLastNonEmpty ? filtered.Last() : filtered.First();
        //    return chosen.Name.Trim();
        //}
        // ===================== Dispose =====================

        public void Dispose()
        {
            DisposeInternal();
        }

        private void DisposeInternal()
        {
            try { _automation?.Dispose(); } catch { }
            _automation = null;

            try { _app?.Dispose(); } catch { }
            _app = null;

            //_cachedNodes = Array.Empty<AutomationElement>();
            _cachedNodes = new List<AutomationElement>();
            _elementsById = null;
            _mainWindow = null;
        }

        #region Dành riêng cho KLAS
        //public async Task<AutomationElement?> WaitWindowByNameOrClassAsync(CancellationToken ct, TimeSpan timeout, params string[] namesOrClasses)
        //{
        //    var desktop = _automation.GetDesktop();
        //    var cf = _automation.ConditionFactory;
        //    var end = DateTime.UtcNow + timeout;

        //    while (DateTime.UtcNow < end && !ct.IsCancellationRequested)
        //    {
        //        var wins = desktop.FindAllChildren(cf.ByControlType(ControlType.Window));
        //        foreach (var w in wins)
        //        {
        //            var n = w.Name ?? string.Empty;
        //            var cls = w.ClassName ?? string.Empty;

        //            if (namesOrClasses.Any(key =>
        //                key.Equals(n, StringComparison.OrdinalIgnoreCase) ||
        //                key.Equals(cls, StringComparison.OrdinalIgnoreCase)))
        //                return w;
        //        }

        //        await Task.Delay(100, ct);
        //    }
        //    return null;
        //}

        //public AutomationElement? FindFileNameEdit(AutomationElement dialog)
        //{
        //    if (dialog == null) return null;
        //    var cf = _automation.ConditionFactory;

        //    // Tìm mọi Edit
        //    var edits = dialog.FindAllDescendants(cf.ByControlType(ControlType.Edit));
        //    if (edits == null || edits.Length == 0) return null;

        //    // Theo AutomationId quen thuộc
        //    var byId = edits.FirstOrDefault(x =>
        //        x.AutomationId == "FileNameControlHost" || // Win10/11 common item dialog
        //        x.AutomationId == "1148" ||                // legacy #32770
        //        x.AutomationId == "1001" ||
        //        x.AutomationId == "FileNameControl");
        //    if (byId != null) return byId;

        //    // Theo Name đa ngôn ngữ
        //    var byName = edits.FirstOrDefault(x =>
        //    {
        //        var n = x.Name ?? string.Empty;
        //        return n.StartsWith("File name", StringComparison.OrdinalIgnoreCase) ||
        //               n.StartsWith("Nombre de archivo", StringComparison.OrdinalIgnoreCase);
        //    });
        //    if (byName != null) return byName;

        //    // Fallback: chọn Edit có diện tích lớn nhất
        //    return edits.OrderByDescending(e => e.BoundingRectangle.Area()).FirstOrDefault();
        //}

        //// Clear và dán (ổn định hơn gõ ký tự)
        //public void FocusAndPaste(AutomationElement el, string text)
        //{
        //    if (el == null) return;
        //    el.Focus();
        //    var br = el.BoundingRectangle;
        //    if (!br.IsEmpty) Mouse.Click(br.Center());
        //    Thread.Sleep(50);

        //    // Ctrl+A, Delete
        //    Keyboard.Press(VirtualKeyShort.CONTROL);
        //    Keyboard.Press(VirtualKeyShort.KEY_A);
        //    Keyboard.Release(VirtualKeyShort.KEY_A);
        //    Keyboard.Release(VirtualKeyShort.CONTROL);
        //    Keyboard.Type(VirtualKeyShort.DELETE);

        //    // Clipboard + Ctrl+V
        //    Clipboard.SetText(text ?? string.Empty);
        //    Keyboard.Press(VirtualKeyShort.CONTROL);
        //    Keyboard.Press(VirtualKeyShort.KEY_V);
        //    Keyboard.Release(VirtualKeyShort.KEY_V);
        //    Keyboard.Release(VirtualKeyShort.CONTROL);
        //}

        //public async Task<(AutomationElement dialog, AutomationElement fileNameEdit)?> WaitCommonFileDialogAsync(CancellationToken ct, TimeSpan timeout)
        //{
        //    var desktop = _automation.GetDesktop();
        //    var cf = _automation.ConditionFactory;
        //    var deadline = DateTime.UtcNow + timeout;
        //    string[] editIds = { "FileNameControlHost", "1148", "1001", "FileNameControl" };

        //    while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
        //    {
        //        var edits = desktop.FindAllDescendants(cf.ByControlType(ControlType.Edit));
        //        if (edits?.Length > 0)
        //        {
        //            var cand = edits.FirstOrDefault(e => editIds.Contains(e.AutomationId))
        //                       ?? edits.FirstOrDefault(e =>
        //                           (e.Name ?? "").StartsWith("File name", StringComparison.OrdinalIgnoreCase) ||
        //                           (e.Name ?? "").StartsWith("Nombre de archivo", StringComparison.OrdinalIgnoreCase));
        //            if (cand != null)
        //            {
        //                // Lần ngược lên cửa sổ dialog
        //                var parent = cand.Parent;
        //                AutomationElement dlg = null;
        //                while (parent != null)
        //                {
        //                    if (parent.ControlType == ControlType.Window || parent.ControlType == ControlType.Pane)
        //                    {
        //                        dlg = parent;
        //                        break;
        //                    }
        //                    parent = parent.Parent;
        //                }
        //                if (dlg != null)
        //                {
        //                    try { dlg.Focus(); } catch { }
        //                    return (dlg, cand);
        //                }
        //            }
        //        }
        //        await Task.Delay(150, ct);
        //    }
        //    return null;
        //}
        //public async Task<bool> WaitSuccessDialogAsync(CancellationToken ct, TimeSpan timeout)
        //{
        //    // tuỳ app: thêm/đổi tiêu đề/cls hợp thực tế
        //    var okWin = await WaitWindowByNameOrClassAsync(
        //        ct, timeout,
        //        "[90007]", "OK", "Success", "Export Successful", "Guardado", "Éxito", "Completado", "#32770");

        //    if (okWin == null) return false;

        //    var okBtn = FindButtonByNamesOrIds(okWin, "OK", "Aceptar", "Close");
        //    if (okBtn?.Patterns.Invoke.IsSupported == true)
        //        okBtn.Patterns.Invoke.Pattern.Invoke();
        //    else if (okBtn != null && !okBtn.BoundingRectangle.IsEmpty)
        //        FlaUI.Core.Input.Mouse.Click(okBtn.BoundingRectangle.Center());
        //    else
        //        FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.RETURN);

        //    return true;
        //}

        //public AutomationElement? FindButtonByNamesOrIds(AutomationElement scope, params string[] namesOrIds)
        //{
        //    if (scope == null) return null;
        //    if (namesOrIds == null || namesOrIds.Length == 0) return null;

        //    var cf = _automation.ConditionFactory;

        //    try
        //    {
        //        // Tìm tất cả button trong phạm vi được cung cấp
        //        var buttons = scope.FindAllDescendants(cf.ByControlType(ControlType.Button));
        //        if (buttons == null || buttons.Length == 0)
        //            return null;

        //        // 1️⃣ Ưu tiên tìm theo AutomationId (ổn định nhất)
        //        foreach (var id in namesOrIds)
        //        {
        //            var match = buttons.FirstOrDefault(b =>
        //                string.Equals(b.AutomationId, id, StringComparison.OrdinalIgnoreCase));
        //            if (match != null)
        //                return match;
        //        }

        //        // 2️⃣ Sau đó tìm theo Name (UI text)
        //        foreach (var name in namesOrIds)
        //        {
        //            var match = buttons.FirstOrDefault(b =>
        //                string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
        //            if (match != null)
        //                return match;
        //        }

        //        // 3️⃣ Fallback: nếu chưa tìm được, thử các nút có Name chứa cụm từ đó
        //        foreach (var key in namesOrIds)
        //        {
        //            var match = buttons.FirstOrDefault(b =>
        //                (b.Name ?? string.Empty).Contains(key, StringComparison.OrdinalIgnoreCase));
        //            if (match != null)
        //                return match;
        //        }

        //        // 4️⃣ Fallback cuối: chọn button có diện tích lớn nhất (thường là nút chính)
        //        var biggest = buttons
        //            .OrderByDescending(b => b.BoundingRectangle.Width * b.BoundingRectangle.Height)
        //            .FirstOrDefault();

        //        return biggest;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}




        // ===== Tên/ID phổ biến cần nhận diện =====
        private static readonly string[] FileNameEditIds = { "FileNameControlHost", "1148", "1001", "FileNameControl" };
        private static readonly string[] FileNameEditNames = { "File name", "Nombre de archivo", "Nombre del archivo", "Nombre de ficheiro", "Nome do arquivo", "Dateiname" };
        private static readonly string[] SaveButtonNames = { "Save", "Guardar", "Guardar como", "Save As" };
        private static readonly string[] CancelButtonNames = { "Cancel", "Cancelar" };
        private static readonly string[] OkButtonNames = { "OK", "Aceptar", "Aceptar.", "Aceptar ", "Aceptar…", "Sí", "Si", "Yes", };
        /// <summary>
        /// Sau khi bạn click excelButton, gọi hàm này để đợi đúng Save dialog của app đích.
        /// </summary>
        public async Task<(AutomationElement dialog, AutomationElement fileNameEdit, AutomationElement saveButton)?> WaitTargetSaveDialogAsync(CancellationToken ct, TimeSpan timeout)
        {
            var desktop = _automation.GetDesktop();
            var cf = _automation.ConditionFactory;
            var deadline = DateTime.UtcNow + timeout;


            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                // Tìm các cửa sổ thuộc process đích
                var windows = desktop.FindAllChildren(
                    cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Pane)));

                foreach (var win in windows)
                {
                    if (win == null || !win.IsEnabled)
                        continue;

                    win.Properties.ProcessId.TryGetValue(out var pid);
                    if (pid != _processId)
                        continue; // chỉ lấy cửa sổ thuộc process app đích

                    // Có Edit (ô nhập file name)
                    var edits = win.FindAllDescendants(cf.ByControlType(ControlType.Edit));
                    var fileNameEdit = edits?.FirstOrDefault(e =>
                        (e.Name ?? "").Contains("File", StringComparison.OrdinalIgnoreCase) ||
                        (e.Name ?? "").Contains("Nombre", StringComparison.OrdinalIgnoreCase));

                    if (fileNameEdit == null)
                        continue;

                    // Có nút Save
                    var buttons = win.FindAllDescendants(cf.ByControlType(ControlType.Button));
                    var saveBtn = buttons?.FirstOrDefault(b =>
                    {
                        var name = b.Name ?? "";
                        return name.Equals("Save", StringComparison.OrdinalIgnoreCase)
                               || name.Equals("Guardar", StringComparison.OrdinalIgnoreCase);
                    });

                    if (saveBtn == null)
                        continue;

                    try { win.Focus(); } catch { }
                    return (win, fileNameEdit, saveBtn);
                }

                await Task.Delay(150, ct).ConfigureAwait(false);
            }

            return null;
        }

        private static async Task<bool> TryClick(AutomationElement el)
        {
            if (el == null) return false;
            // Invoke
            var inv = el.Patterns.Invoke.PatternOrDefault;
            if (inv != null) { inv.Invoke(); return true; }

            // SelectionItem
            var sel = el.Patterns.SelectionItem.PatternOrDefault;
            if (sel != null) { sel.Select(); return true; }

            // ExpandCollapse (ít khi dùng cho nút, nhưng để phòng hờ)
            var exp = el.Patterns.ExpandCollapse.PatternOrDefault;
            if (exp != null) { exp.Expand(); return true; }

            // Fallback: Click theo tọa độ
            try { el.Click(); return true; } catch { }
            return false;
        }
        public async Task<bool> WaitNextOkDialogAsync(CancellationToken ct, TimeSpan timeout, IntPtr? lastClosedDialogHwnd = null)
        {
            var desktop = _automation.GetDesktop();
            var cf = _automation.ConditionFactory;
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                var tops = desktop.FindAllChildren(
                    cf.ByControlType(ControlType.Window).Or(cf.ByControlType(ControlType.Pane)));

                foreach (var win in tops)
                {
                    if (win == null || !win.IsEnabled) continue;

                    // Cùng ProcessId với app đích
                    win.Properties.ProcessId.TryGetValue(out var pid);
                    if (pid != _processId) continue;

                    // Tránh bắt lại chính dialog cũ (nếu UIA chưa refresh kịp)
                    var hwnd = (IntPtr)win.Properties.NativeWindowHandle.Value;
                    if (lastClosedDialogHwnd.HasValue && hwnd == lastClosedDialogHwnd.Value)
                        continue;

                    // Tìm nút OK (hoặc Aceptar / Yes / Sí)
                    var buttons = win.FindAllDescendants(cf.ByControlType(ControlType.Button));
                    var okBtn = buttons?.FirstOrDefault(b =>
                    {
                        var name = b.Name ?? string.Empty;
                        // so sánh chặt chẽ theo Equals; nếu muốn nới lỏng có thể dùng IndexOf
                        return OkButtonNames.Any(n => name.Equals(n, StringComparison.OrdinalIgnoreCase));
                    });

                    if (okBtn != null)
                    {
                        try { win.Focus(); } catch { /* ignore */ }
                        var clicked = await TryClick(okBtn);
                        if (clicked) return true;
                    }
                }

                await Task.Delay(500, ct).ConfigureAwait(false);
            }

            return false;
        }
        #endregion
    }
}
