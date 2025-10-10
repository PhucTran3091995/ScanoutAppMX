using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbDept
{
    public int Id { get; set; }

    public string? Dept { get; set; }

    public string? Authority { get; set; }

    public virtual ICollection<TbInternalDefect> TbInternalDefects { get; set; } = new List<TbInternalDefect>();

    public virtual ICollection<TbUser> TbUsers { get; set; } = new List<TbUser>();
}
