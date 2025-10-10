using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbRomHistory
{
    public int Id { get; set; }

    public int? RomId { get; set; }

    public int? ActionBy { get; set; }

    public DateTime? PrintAt { get; set; }

    public string? ActionType { get; set; }

    public DateTime? ActionAt { get; set; }

    public string? ActionContent { get; set; }

    public virtual TbUser? ActionByNavigation { get; set; }
}
