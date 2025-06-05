namespace FetchFiles.Common;

public interface IUnitProvider : IAsyncDisposable
{
    Task Initialize();
    IAsyncEnumerable<BackupFile> ListFiles();
    Task<Stream> GetFileStream(BackupFile file);
    Task FetchFile(BackupFile file, Stream destination);
}