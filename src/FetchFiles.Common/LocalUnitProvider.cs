namespace FetchFiles.Common;

using System.Text.RegularExpressions;

public sealed class LocalUnitProvider : AbstractUnitProvider
{
    private DirectoryInfo location = null!;

    public LocalUnitProvider(Unit unit)
        : base(unit)
    {
    }

    public override Task Initialize()
    {
        if (this.Unit.Location == null)
        {
            throw new InvalidOperationException("Cannot initialize local unit without location");
        }

        this.location = new DirectoryInfo(this.Unit.Location);
        return Task.CompletedTask;
    }

    public async override IAsyncEnumerable<BackupFile> ListFiles()
    {
        foreach (var file in location.EnumerateFiles())
        {
            string? id = null;
            bool keep = this.EvaluateFileName(file.Name, out id);
            if (keep)
            {
                yield return new BackupFile(this.Unit.Name, this.Unit.Location!, file.Name, id)
                {
                    Length = file.Length,
                    LastWriteTimeUtc = file.LastWriteTimeUtc,
                };
            }
        }
    }

    public override Task<Stream> GetFileStream(BackupFile file)
    {
        var path = Path.Combine(this.Unit.Location!, file.FileName);
        Stream stream = new FileStream(path, FileMode.Open,  FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public override async Task FetchFile(BackupFile file, Stream destination)
    {
        var path = Path.Combine(this.Unit.Location!, file.FileName);
        await using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var length = file.Length ?? stream.Length;
            if (length >= 0)
            {
                destination.SetLength(length);
            }

            await stream.CopyToAsync(destination);
        }
    }
}
