using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbAoiNg
{
    public int Id { get; set; }

    public string ArrayId { get; set; } = null!;

    public string Pid { get; set; } = null!;

    public int ArrayIndex { get; set; }

    public string Component { get; set; } = null!;

    public string InspType { get; set; } = null!;

    public string UserResult { get; set; } = null!;

    public DateTime EndTime { get; set; }

    public DateOnly EndDate { get; set; }
}
