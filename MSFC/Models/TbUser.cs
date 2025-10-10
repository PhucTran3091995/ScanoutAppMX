using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbUser
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public int? RoleId { get; set; }

    public string? Position { get; set; }

    public string? Active { get; set; }

    public string? Email { get; set; }

    public string? FullName { get; set; }

    public string? PasswordHash { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual TbDept? Role { get; set; }

    public virtual ICollection<TbInternalDefect> TbInternalDefects { get; set; } = new List<TbInternalDefect>();

    public virtual ICollection<TbRomHistory> TbRomHistories { get; set; } = new List<TbRomHistory>();
}
