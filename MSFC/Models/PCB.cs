using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSFC.Models
{
    public class PCB
    {
        public string PID { get; set; }
        public string EBR { get; set; }
        public string WO { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string Progress { get; set; }
        public string Buyer {  get; set; }
        public string Inspector1 { get; set; }
        public string Inspector2 { get; set; }
    }
}
