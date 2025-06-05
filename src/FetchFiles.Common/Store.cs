namespace FetchFiles.Common;

public sealed class Store
{
    public string Name { get; set; } = null!;
    public string? Type { get; set; }
    public string? Location { get; set; }
}
