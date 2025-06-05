namespace FetchFiles.Common;

using Newtonsoft.Json;
using System.Text;

public sealed class LocalStoreProvider : IStoreProvider
{
    private readonly Store store;

    public LocalStoreProvider(Store store)
    {
        this.store = store;
    }

    public string Name { get => this.store.Name; }

    public Task Initialize()
    {
        if (this.store.Location == null)
        {
            throw new InvalidOperationException("Cannot initialize local store without location");
        }

        var location = new DirectoryInfo(this.store.Location);
        if (!location.Exists)
        {
            location.Create();
        }

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<BackupFile> ListFiles()
    {
        var location = new DirectoryInfo(this.store.Location!);
        foreach (var unitDirectory in location.EnumerateDirectories())
        {
            foreach (var unitFile in unitDirectory.EnumerateFiles("*.manifest"))
            {
                var manifestString = await File.ReadAllTextAsync(unitFile.FullName, Encoding.UTF8);
                var manifest = JsonConvert.DeserializeObject<BackupFile>(manifestString);
                if (manifest != null)
                {
                    yield return manifest;
                }
            }
        }
    }

    public ValueTask<bool> Accepts(BackupFile file)
    {
        return ValueTask.FromResult(true);
    }

    public async Task Store(BackupFile file, IUnitProvider source)
    {
        var unitLocation = new DirectoryInfo(Path.Combine(this.store.Location!, file.Unit));
        if (!unitLocation.Exists)
        {
            unitLocation.Create();
        }

        var filePath = Path.Combine(unitLocation.FullName, file.FileName);
        Console.Out.WriteLine($"Store {this.store.Name} will keep file {file.ToString()}");
        var manifestString = JsonConvert.SerializeObject(file);
        var manifestPath = filePath + ".manifest";
        await File.WriteAllTextAsync(manifestPath, manifestString, Encoding.UTF8);

        await using (var destination = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
        {
            await source.FetchFile(file, destination);
        }

        if (file.LastWriteTimeUtc != null)
        {
            File.SetAttributes(filePath, FileAttributes.ReadOnly);
            File.SetLastWriteTimeUtc(filePath, file.LastWriteTimeUtc.Value);
        }
    }

    public async Task<BackupFileState> GetFileInfo(string unit, string fileName)
    {
        BackupFile? file = new BackupFile(unit, null!, fileName, null);
        var state = new BackupFileState(file);
        
        var unitLocation = new DirectoryInfo(Path.Combine(this.store.Location!, unit));
        if (!unitLocation.Exists)
        {
            return state;
        }

        var basePath = Path.Combine(unitLocation.FullName, fileName);
        var manifestPath = basePath + ".manifest";
        if (!File.Exists(manifestPath))
        {
            return state;
        }

        var manifestString = await File.ReadAllTextAsync(manifestPath, Encoding.UTF8);
        file = JsonConvert.DeserializeObject<BackupFile>(manifestString) ?? throw new InvalidOperationException("Manifest is invalid");

        var info = new FileInfo(basePath);
        if (info.Exists)
        {
            state = new BackupFileState(file);
            state.Exists = true;
            state.State = new BackupFile(unit, info.DirectoryName!, info.Name, null)
            {
                Length = info.Length,
                LastWriteTimeUtc = info.LastWriteTimeUtc,
            };
        }
        
        return state;
    }

    public string ToShortString()
    {
        return $"Store: {this.Name}";
    }

    public override string ToString()
    {
        return $"Store: {this.Name}";
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
