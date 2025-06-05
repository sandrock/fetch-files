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

        var basePath = Path.Combine(unitLocation.FullName, file.FileName);
        Console.Out.WriteLine($"Store {this.store.Name} will keep file {file.ToString()}");
        var manifestString = JsonConvert.SerializeObject(file);
        await File.WriteAllTextAsync(basePath + ".manifest", manifestString, Encoding.UTF8);

        await using (var sourceStream = await source.GetFileStream(file))
        await using (var destinationStream = new FileStream(basePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
        {
            destinationStream.SetLength(0L);
            await sourceStream.CopyToAsync(destinationStream);
        }
    }
}
