namespace FetchFiles.Common;

using System.Text.RegularExpressions;

public sealed class LocalUnitProvider : IUnitProvider
{
    private readonly Unit unit;
    private DirectoryInfo location = null!;

    public LocalUnitProvider(Unit unit)
    {
        this.unit = unit;
    }

    public Task Initialize()
    {
        if (this.unit.Location == null)
        {
            throw new InvalidOperationException("Cannot initialize local unit without location");
        }

        this.location = new DirectoryInfo(this.unit.Location);
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<BackupFile> ListFiles()
    {
        var fileFormat = this.unit.FileFormat != null ? new Regex(this.unit.FileFormat) : null;
        foreach (var file in location.EnumerateFiles())
        {
            bool keep = false;
            string? id = null;
            if (fileFormat != null)
            {
                var fileFormatMatch = fileFormat.Match(file.Name);
                keep = fileFormatMatch.Success;
                if (keep)
                {
                    id = fileFormatMatch.Groups["id"].Value;
                }
            }
            else
            {
                keep = true;
            }

            if (keep)
            {
                yield return new BackupFile(this.unit.Name, this.unit.Location!, file.Name, id);
            }
        }
    }

    public Task<Stream> GetFileStream(BackupFile file)
    {
        var path = Path.Combine(this.unit.Location!, file.FileName);
        Stream stream = new FileStream(path, FileMode.Open,  FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }
}
