using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbInternalDefect
{
    public int Id { get; set; }

    public string? Pid { get; set; }

    public DateOnly? DefectDate { get; set; }

    public int? RepairMan { get; set; }

    public string? Process { get; set; }

    public string? DefectName { get; set; }

    public string? Location { get; set; }

    public string? Side { get; set; }

    public int? DefectImage { get; set; }

    public string? Cause { get; set; }

    public int? IssueOwner { get; set; }

    public string? ActionType { get; set; }

    public string? ActionContent { get; set; }

    public DateOnly? ActionDate { get; set; }

    public virtual TbDept? IssueOwnerNavigation { get; set; }

    public virtual TbUser? RepairManNavigation { get; set; }

    public virtual ICollection<TbDefectImage> TbDefectImages { get; set; } = new List<TbDefectImage>();
}
