using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbAoiPid
{
    public int Id { get; set; }

    public string Line { get; set; } = null!;

    public string WorkFace { get; set; } = null!;

    public string WorkOrder { get; set; } = null!;

    public string ArrayId { get; set; } = null!;

    public string Pid { get; set; } = null!;

    public string ProgramName { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string MachineResult { get; set; } = null!;

    public string UserResult { get; set; } = null!;

    public DateOnly EndDate { get; set; }

    public DateTime EndTime { get; set; }
}
