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
}
