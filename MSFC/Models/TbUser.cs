using System;
using System.Collections.Generic;

namespace MSFC.Models;

public partial class TbUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Dept { get; set; } = null!;

    public string Factory { get; set; } = null!;

    public string Active { get; set; } = null!;
}
