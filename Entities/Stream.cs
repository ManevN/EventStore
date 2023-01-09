using System;
using System.Collections.Generic;

namespace WebApp.Entities;

public partial class Stream
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public long Version { get; set; }

    public virtual ICollection<Event> Events { get; } = new List<Event>();
}
