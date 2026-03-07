using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Sport
{
    public int SportId { get; set; }

    public string Name { get; set; } = null!;

    public int? TeamMin { get; set; }

    public int? TeamMax { get; set; }

    public int? ImageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SportImage? Image { get; set; }

    public virtual ICollection<MatchPost> MatchPosts { get; set; } = new List<MatchPost>();
}
