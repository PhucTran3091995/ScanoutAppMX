using System;
using System.Collections.Generic;

namespace MSFC.Models;

/// <summary>
/// table to record scan out pid
/// </summary>
public partial class TbScanOut
{
    public int Id { get; set; }

    public string? ClientId { get; set; }

    public long? TagId { get; set; }

    public string? Pid { get; set; }

    public string? ModelName { get; set; }

    public string? ModelSuffix { get; set; }

    public string? WorkOrder { get; set; }

    public string? PartNo { get; set; }

    public DateTime? ScanAt { get; set; }

    public DateTime? PrintAt { get; set; }

    public int? Qty { get; set; }

    public string? FirstInspector { get; set; }

    public string? SecondInspector { get; set; }

    public string? _4m { get; set; }

    public DateOnly? ScanDate { get; set; }

    public DateOnly? PrintDate { get; set; }
}
