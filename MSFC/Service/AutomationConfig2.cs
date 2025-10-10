using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSFC.Service
{
    public class AutomationConfig2
    {
        public string ProcessName { get; set; }
        public ControlNames Controls { get; set; }
    }
    public class ControlNames
    {
        public string ClearButton { get; set; }
        public string ResultQty { get; set; }
        public string RemainQty { get; set; }
        public string OK { get; set; }
        public string PID { get; set; }
        public string ModelSuffixUc { get; set; }
        public string ModelSuffixContent { get; set; }
        public string WorkOrderUc { get; set; }
        public string WoContent { get; set; }
        public string ResultUc { get; set; }
        public string ResultText { get; set; }
    }
    public class KlasUiConfig
    {
        public string AppUrl { get; set; }
        public string ProcessName { get; set; }
        public string actionDelaySec { get; set; }
        public string waitCmdQueryDelaySec { get; set; }
        public string KlasScanIntervalSec { get; set; }

        public string waitExcelDelaySec { get; set; }
        public string saveFolder { get; set; }

        public string KlasAcc { get; set; }
        public string KlasPassword { get; set; }

        public KlasControlNames Controls { get; set; }
    }
    public class KlasControlNames
    {
        public string PrintManagementMenu { get; set; }
        public string LabelPrintLogMenu { get; set; }
        public string SearchButton { get; set; }
        public string ExcelButton { get; set; }
        public string savePath { get; set; }
        public string saveButton { get; set; }
        public string saveOkButton { get; set; }
        public string overideButton { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string LoginBtn { get; set; }
        public string LogoutBtn { get; set; }

    }
}
