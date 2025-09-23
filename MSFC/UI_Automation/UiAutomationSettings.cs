using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSFC.UI_Automation
{
    public sealed class UiAutomationSettings
    {
        public string ProcessName { get; set; } = "";
        public string ProcIdContains { get; set; } = ""; // để check IsScanoutUI
        public TimingsSection Timings { get; set; } = new();
        public ControlIdsSection ControlIds { get; set; } = new();

        public sealed class TimingsSection
        {
            public int PollIntervalMs { get; set; } = 100;
            public int EventDebounceMs { get; set; } = 100;
            public int ReadTimeoutMs { get; set; } = 5000;
            public int WatchdogIntervalMs { get; set; } = 100;
        }

        public sealed class ControlIdsSection
        {
            public string PidTextBox { get; set; } = "";
            public string EbrTextBox { get; set; } = "";
            public string WoTextBox { get; set; } = "";
            public string ResultTextBox { get; set; } = "";
            public string MessageTextBox { get; set; } = "";
            public string ProgressText { get; set; } = "";
            public string ProcessIdText { get; set; } = "";
            public string ButtonText { get; set; } = "";
        }
    }

}
