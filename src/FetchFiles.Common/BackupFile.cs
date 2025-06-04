namespace FetchFiles.Common;

public sealed class BackupFile
{
    public BackupFile(string unit, string location, string fileName, string? id)
    {
        this.Unit = unit;
        this.Location = location;
        this.FileName = fileName;
        this.Id = id;
    }

    public string Unit { get; }
    public string Location { get; }
    public string FileName { get; }
    public string? Id { get; }

    public override string ToString()
    {
        return $"{nameof(this.Unit)}: {this.Unit}, {nameof(this.Location)}: {this.Location}, {nameof(this.FileName)}: {this.FileName}, {nameof(this.Id)}: {this.Id}";
    }
}
