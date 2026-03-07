using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class SportImage
{
    public int ImageId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual ICollection<Sport> Sports { get; set; } = new List<Sport>();
}
