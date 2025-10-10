using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbDefectImage
{
    public int Id { get; set; }

    public int? DefectId { get; set; }

    public string? Path { get; set; }

    public virtual TbInternalDefect? Defect { get; set; }
}
