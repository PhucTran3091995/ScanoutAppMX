using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbTagTemp
{
    public int Id { get; set; }

    public string? Client { get; set; }

    public string? Pid { get; set; }

    public string? WorkOrder { get; set; }

    public DateTime? ScanAt { get; set; }

    public string? Model { get; set; }

    public string? PartNo { get; set; }
}
