using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbStationMonitoring
{
    public int Id { get; set; }

    public string? IpAddress { get; set; }

    public string? StationName { get; set; }

    /// <summary>
    /// RUNNING
    /// OFF
    /// </summary>
    public string? Status { get; set; }

    public string? Note { get; set; }

    public DateTime? UpdateAt { get; set; }
}
