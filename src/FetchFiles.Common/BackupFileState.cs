
namespace FetchFiles.Common;

using System;

public sealed class BackupFileState
{
    public BackupFileState(BackupFile file)
    {
        this.File = file;
    }

    public BackupFile File { get; }

    public bool Exists { get; set; }

    public BackupFile? State { get; set; }
}
