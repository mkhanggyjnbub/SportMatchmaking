using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Report
{
    public long ReportId { get; set; }

    public int ReporterUserId { get; set; }

    public byte TargetType { get; set; }

    public long? TargetPostId { get; set; }

    public int? TargetUserId { get; set; }

    public byte ReasonCode { get; set; }

    public string? Details { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? Resolution { get; set; }

    public virtual AppUser ReporterUser { get; set; } = null!;

    public virtual MatchPost? TargetPost { get; set; }

    public virtual AppUser? TargetUser { get; set; }
}
