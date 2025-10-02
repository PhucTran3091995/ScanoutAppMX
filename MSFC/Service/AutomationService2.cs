using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
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

        void ClickTo(string automationId);
        void SetText(string automationId, string text);
        string GetText(string automationId);
        string GetText(string parentAutomationId, string childAutomationId, int index = 0); // overload mới

        string SafeGetText(AutomationElement el);

        string GetInnerMessageFromResult(string resultUcId, string resultTextId, bool pickLastNonEmpty = true);
    }

    /// <summary>
    /// Triển khai IAutomationService2 sử dụng FlaUI 5.x (UIA3)
    /// </summary>
    public class AutomationService2 : IAutomationService2
    {
        private AutomationBase _automation;
        private Application _app;
        private Window _mainWindow;

        private IReadOnlyList<AutomationElement> _cachedNodes;
        private readonly List<IDisposable> _eventHandlers = new();

        // Cache theo AutomationId để xử lý duplicate
        private Dictionary<string, List<AutomationElement>> _elementsById;

        // ====== Attach / Scan / Cache =========================================================

        public void AttachToProcess(int processId)
        {
            // Dispose tài nguyên cũ
            _automation?.Dispose();
            _app?.Dispose();

            _automation = new UIA3Automation();
            _app = Application.Attach(processId);
            _mainWindow = _app.GetMainWindow(_automation)
                ?? throw new InvalidOperationException("Cannot get main window of process.");

            CacheAllNodes();
        }

        public void AttachToProcess(string processName)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault()
                ?? throw new InvalidOperationException($"Process '{processName}' not found.");
            AttachToProcess(process.Id);
        }

        private void CacheAllNodes()
        {
            if (_mainWindow == null)
            {
                _cachedNodes = Array.Empty<AutomationElement>();
                _elementsById = new Dictionary<string, List<AutomationElement>>(StringComparer.Ordinal);
                return;
            }

            _cachedNodes = _mainWindow.FindAllDescendants();

            _elementsById = _cachedNodes
                .Where(e => !string.IsNullOrWhiteSpace(e.AutomationId))
                .GroupBy(e => e.AutomationId, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        }

        public void RefreshCache() => CacheAllNodes();

        public IReadOnlyList<AutomationElement> ScanApp()
            => _mainWindow == null ? Array.Empty<AutomationElement>() : _mainWindow.FindAllDescendants();

        // ====== Tìm kiếm Control =============================================================

        /// <summary>Trả về control đầu tiên khớp AutomationId (nếu duplicate sẽ lấy phần tử đầu tiên)</summary>
        public AutomationElement GetControlById(string automationId)
        {
            if (string.IsNullOrWhiteSpace(automationId) || _elementsById == null) return null;
            return _elementsById.TryGetValue(automationId, out var list) ? list.FirstOrDefault() : null;
        }

        /// <summary>Trả về tất cả control khớp AutomationId (xử lý duplicate)</summary>
        public IReadOnlyList<AutomationElement> GetControlsById(string automationId)
        {
            if (string.IsNullOrWhiteSpace(automationId) || _elementsById == null) return Array.Empty<AutomationElement>();
            return _elementsById.TryGetValue(automationId, out var list) ? list : Array.Empty<AutomationElement>();
        }

        /// <summary>Lấy control thứ N theo AutomationId (0-based). Trả null nếu vượt phạm vi.</summary>
        public AutomationElement GetNthById(string automationId, int index)
        {
            var list = GetControlsById(automationId);
            return (index >= 0 && index < list.Count) ? list[index] : null;
        }

        /// <summary>
        /// Lấy control con theo (parentAutomationId, childAutomationId).
        /// Nếu có nhiều parent cùng Id, sẽ trả về match đầu tiên tìm thấy trong từng parent theo thứ tự.
        /// </summary>
        public AutomationElement GetChildControl(string parentAutomationId, string childAutomationId)
            => GetChildControl(parentAutomationId, childAutomationId, 0);

        /// <summary>Overload: lấy control con theo index trong phạm vi parent.</summary>
        public AutomationElement GetChildControl(string parentAutomationId, string childAutomationId, int index)
        {
            if (string.IsNullOrWhiteSpace(parentAutomationId) || string.IsNullOrWhiteSpace(childAutomationId))
                return null;

            var parents = GetControlsById(parentAutomationId);
            if (parents.Count == 0) return null;

            foreach (var parent in parents)
            {
                var matches = parent.FindAllDescendants(cf => cf.ByAutomationId(childAutomationId));
                if (matches != null && matches.Length > index)
                    return matches[index];
            }
            return null;
        }

        /// <summary>
        /// Tìm control theo "đường dẫn" nhiều cấp: ví dụ ("ucA", "ucB", "txtText")
        /// → tìm tất cả "ucA", trong đó tìm "ucB", rồi trong đó tìm "txtText".
        /// </summary>
        public AutomationElement GetControlByPath(params string[] automationIds)
        {
            if (automationIds == null || automationIds.Length == 0) return null;

            var currentLevel = GetControlsById(automationIds[0]).ToList();
            if (currentLevel.Count == 0) return null;

            for (int i = 1; i < automationIds.Length; i++)
            {
                string id = automationIds[i];
                var nextLevel = new List<AutomationElement>();
                foreach (var parent in currentLevel)
                {
                    var found = parent.FindAllDescendants(cf => cf.ByAutomationId(id));
                    if (found != null && found.Length > 0) nextLevel.AddRange(found);
                }
                if (nextLevel.Count == 0) return null;
                currentLevel = nextLevel;
            }

            return currentLevel.FirstOrDefault();
        }

        // ====== Actions ======================================================================

        public void ClickTo(string automationId)
        {
            var control = GetControlById(automationId);
            control?.AsButton()?.Invoke();
        }

        public void SetText(string automationId, string text)
        {
            var control = GetControlById(automationId);
            control?.AsTextBox()?.Enter(text ?? string.Empty);
        }

        /// <summary>Overload: SetText theo (parent, child, index)</summary>
        public void SetText(string parentAutomationId, string childAutomationId, string text, int index = 0)
        {
            var el = GetChildControl(parentAutomationId, childAutomationId, index);
            el?.AsTextBox()?.Enter(text ?? string.Empty);
        }

        // ====== Đọc Text =====================================================================

        /// <summary>
        /// GetText theo AutomationId (giữ tương thích). Dùng SafeGetText bên trong.
        /// </summary>
        public string GetText(string automationId)
        {
            var control = GetControlById(automationId);
            return SafeGetText(control);
        }

        /// <summary>Overload: GetText theo (parent, child, index)</summary>
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
                // TextBox/Edit
                if (el.ControlType == ControlType.Edit && el.Patterns.Value.IsSupported)
                    return el.Patterns.Value.Pattern.Value ?? string.Empty;

                // Control khác nhưng hỗ trợ ValuePattern (ví dụ ComboBox editable)
                if (el.Patterns.Value.IsSupported)
                    return el.Patterns.Value.Pattern.Value ?? string.Empty;

                // Document / RichText
                if (el.Patterns.Text.IsSupported)
                {
                    var t = el.Patterns.Text.Pattern.DocumentRange.GetText(int.MaxValue);
                    return t?.TrimEnd('\r', '\n') ?? string.Empty;
                }

                // WPF TextBlock / Label
                if (el.ControlType == ControlType.Text && !string.IsNullOrEmpty(el.Name))
                    return el.Name;

                // Legacy
                if (el.Patterns.LegacyIAccessible.IsSupported)
                {
                    var lp = el.Patterns.LegacyIAccessible.Pattern;
                    if (!string.IsNullOrEmpty(lp.Value)) return lp.Value;
                    if (!string.IsNullOrEmpty(lp.Name)) return lp.Name;
                }

                // Fallback Name
                if (!string.IsNullOrEmpty(el.Name)) return el.Name;

                // Tìm text con
                var txt = el.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text));
                if (txt != null && !string.IsNullOrEmpty(txt.Name)) return txt.Name;

                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SafeGetText error: {ex}");
                return null;
            }
        }

        public string GetInnerMessageFromResult(string resultUcId, string resultTextId, bool pickLastNonEmpty = true)
        {
            var uc = GetControlById(resultUcId);
            if (uc == null) return null;

            // Tất cả TextBlock (ControlType.Text) bên trong ucResult
            var texts = uc.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text));
            if (texts == null || texts.Length == 0) return null;

            // Lọc: bỏ chính txtResult (cái hiển thị OK/NG) và bỏ rỗng
            var filtered = texts
                .Where(t => !string.Equals(t.AutomationId, resultTextId, StringComparison.Ordinal))
                .Where(t => !string.IsNullOrWhiteSpace(t.Name))
                .ToList();

            if (filtered.Count == 0) return null;

            // Thường message là TextBlock “sâu/đằng sau” → chọn cái cuối
            var chosen = pickLastNonEmpty ? filtered.Last() : filtered.First();
            return chosen.Name.Trim();
        }


        // ====== Dispose ======================================================================

        public void Dispose()
        {
            foreach (var h in _eventHandlers)
                h.Dispose();
            _eventHandlers.Clear();

            _automation?.Dispose();
            _automation = null;

            _app?.Dispose();
            _app = null;

            _cachedNodes = Array.Empty<AutomationElement>();
            _elementsById = null;
            _mainWindow = null;
        }
    }
}
