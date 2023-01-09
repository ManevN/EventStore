using System;
using System.Collections.Generic;

namespace WebApp.Entities;

public partial class Event
{
    public Guid Id { get; set; }

    public string Data { get; set; } = null!;

    public Guid Streamfk { get; set; }

    public string Type { get; set; } = null!;

    public long Version { get; set; }

    public DateTime Created { get; set; }

    public virtual Stream StreamfkNavigation { get; set; } = null!;
}
