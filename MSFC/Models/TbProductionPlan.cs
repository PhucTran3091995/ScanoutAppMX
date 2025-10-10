using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbProductionPlan
{
    public string GmesWo { get; set; } = null!;

    public string? Line { get; set; }

    public string? Lane { get; set; }

    public string? ModelSuffix { get; set; }

    public string? ModelName { get; set; }

    public string? Buyer { get; set; }

    public string? BoardName { get; set; }

    public string? SmtAssyPn { get; set; }

    public string? PcbaAssyPn { get; set; }

    public string? BarePcbPn { get; set; }

    public int? WoQty { get; set; }

    public DateOnly? ProdDate { get; set; }

    public DateTime? ProdTime { get; set; }

    public int Id { get; set; }
}
