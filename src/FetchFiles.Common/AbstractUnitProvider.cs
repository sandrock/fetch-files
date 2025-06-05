namespace FetchFiles.Common;

using System.Text.RegularExpressions;

public abstract class AbstractUnitProvider : IUnitProvider, IAsyncDisposable
{
    private Regex? fileFormatRegex;

    protected AbstractUnitProvider(Unit unit)
    {
        this.Unit = unit;
    }

    protected Unit Unit { get; }

    public abstract Task Initialize();

    public abstract IAsyncEnumerable<BackupFile> ListFiles();

    public abstract Task<Stream> GetFileStream(BackupFile file);

    public abstract Task FetchFile(BackupFile file, Stream destination);

    protected bool EvaluateFileName(string fileName, out string? id)
    {
        if (this.fileFormatRegex == null && this.Unit.FileFormat != null)
        {
            this.fileFormatRegex = new Regex(this.Unit.FileFormat);
        }

        bool keep = false;
        id = null;
        if (this.fileFormatRegex != null)
        {
            var fileFormatMatch = this.fileFormatRegex.Match(fileName);
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

        return keep;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}
