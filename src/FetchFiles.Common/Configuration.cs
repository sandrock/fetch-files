
namespace FetchFiles.Common;

using System;

public sealed record Configuration
{
    public Dictionary<string, Unit>? Units { get; set; }
    public Dictionary<string, Store>? Stores { get; set; }
}