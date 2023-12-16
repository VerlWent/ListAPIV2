using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ListAPI.Model;

public partial class TaskT
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? Done { get; set; }

    public string? Priority { get; set; }
    public DateOnly? Deadline { get; set; }
    //[JsonIgnore]
    public virtual UserT User { get; set; } = null!;
}
