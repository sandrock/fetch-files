namespace FetchFiles.Common;

public sealed class Unit
{
    public string Name { get; set; } = null!;
    public string? Type { get; set; }
    public string? Location { get; set; }
    public string? FileFormat { get; set; }
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

