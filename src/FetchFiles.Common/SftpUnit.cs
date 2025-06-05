
namespace FetchFiles.Common;

using FetchFiles.Common.Internals;
using Renci.SshNet;
using System;
using System.Diagnostics;
using System.Globalization;

public sealed class SftpUnit : AbstractUnitProvider
{
    private SftpClient? clientInstance;

    public SftpUnit(Unit unit)
        : base(unit)
    {
    }

    public override Task Initialize()
    {
        
        return Task.CompletedTask;
    }

    public override async IAsyncEnumerable<BackupFile> ListFiles()
    {
        if (string.IsNullOrWhiteSpace(this.Unit.Host))
        {
            throw new InvalidOperationException("Host cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(this.Unit.Location))
        {
            throw new InvalidOperationException("Location cannot be null or empty.");
        }

        var cancellationTokenSource = new CancellationTokenSource();
        var cancel = cancellationTokenSource.Token;
        var client = await this.GetReadySftpClient(cancel);
        await foreach (var file in client.ListDirectoryAsync(this.Unit.Location, cancel))
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

    public override async Task<Stream> GetFileStream(BackupFile file)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancel = cancellationTokenSource.Token;
        var client = await this.GetReadySftpClient(cancel);
        var path = this.Unit.Location + "/" + file.FileName;
        var buffer = new StreamBuffer();
        var task = Task.Factory.FromAsync(
            client.BeginDownloadFile(path, buffer),
            client.EndDownloadFile);
        return buffer;
    }

    public override async Task FetchFile(BackupFile file, Stream destination)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancel = cancellationTokenSource.Token;
        var client = await this.GetReadySftpClient(cancel);
        var path = this.Unit.Location + "/" + file.FileName;

        var length = file.Length ?? 0L;
        if (length >= 0)
        {
            destination.SetLength(length);
        }

        var task = Task.Factory.FromAsync(
            client.BeginDownloadFile(path, destination),
            client.EndDownloadFile);
        await task;
        Console.WriteLine("SftpUnit: fetch ended");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.clientInstance?.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task<SftpClient> GetReadySftpClient(CancellationToken cancel)
    {
        if (this.clientInstance == null)
        {
            this.clientInstance = this.CreateSftpClient();
            await this.clientInstance.ConnectAsync(cancel);
        }
        
        return this.clientInstance;
    }

    private SftpClient CreateSftpClient()
    {
        return new SftpClient(this.Unit.Host!, this.Unit.Port ?? 22, this.Unit.Username, this.Unit.Password);
    }
}