using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MSFC.Dto
{
    public class ManufacturingTagDto
    {
        public string ModelName { get; set; }
        public string Date { get; set; }
        public string PartNo { get; set; }
        public int Quantity { get; set; }
        public string Inspector1 { get; set; }
        public string Inspector2 { get; set; }
        public string FourM { get; set; }
        public string Process { get; set; } // 4M
        public long TagId {  get; set; }
        public List<string> PidList { get; set; } = new List<string>();
    }
}
