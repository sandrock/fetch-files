namespace FetchFiles.Common;

public interface IUnitProvider
{
    Task Initialize();
    IAsyncEnumerable<BackupFile> ListFiles();
    Task<Stream> GetFileStream(BackupFile file);
}