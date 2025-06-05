namespace FetchFiles.Common;

public interface IStoreProvider
{
    Task Initialize();
    IAsyncEnumerable<BackupFile> ListFiles();
    ValueTask<bool> Accepts(BackupFile file);
    Task Store(BackupFile file, IUnitProvider source);
}
