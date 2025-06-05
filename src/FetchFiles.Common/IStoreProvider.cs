namespace FetchFiles.Common;

public interface IStoreProvider : IAsyncDisposable
{
    string Name { get; }
    Task Initialize();
    IAsyncEnumerable<BackupFile> ListFiles();
    ValueTask<bool> Accepts(BackupFile file);
    Task Store(BackupFile file, IUnitProvider source);
    Task<BackupFileState> GetFileInfo(string unit, string fileName);
    string ToShortString();
}
