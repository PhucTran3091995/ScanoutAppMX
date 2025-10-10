using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbRomDatum
{
    public int Id { get; set; }

    public string? Model { get; set; }

    public string? Board { get; set; }

    public string? ModelSuffix { get; set; }

    public string? SmtAssyPn { get; set; }

    public string? PcbPn { get; set; }

    public string? PcbaAssyPn { get; set; }

    public string? Stage { get; set; }

    public string? IcName { get; set; }

    public string? IcPn { get; set; }

    public string? Description { get; set; }

    public string? LgChecksum { get; set; }

    public string? HseChecksumDitek { get; set; }

    public string? HseChecksumDedi { get; set; }

    public string? ChecksumTypeDedi { get; set; }

    public string? SwVersion { get; set; }

    public string? EcoNo { get; set; }

    public string? FileName { get; set; }

    public string? ReleaseNoteFile { get; set; }

    public string? SwProgramName { get; set; }

    public string? PDataDedi { get; set; }

    public string? ProgramType { get; set; }

    public string? IcMakerPn { get; set; }

    public string? PackageInfoDedi { get; set; }

    public string? SocketName { get; set; }

    public string? Mc { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public string? FirstWo { get; set; }

    public string? Remarks { get; set; }

    public string? MarkingLaser { get; set; }

    public string? MarkingColor1 { get; set; }

    public string? ColorCode1 { get; set; }

    public string? MarkingColor2 { get; set; }

    public string? ColorCode2 { get; set; }

    public string? Active { get; set; }
}
