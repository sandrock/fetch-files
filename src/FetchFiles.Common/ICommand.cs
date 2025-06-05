
namespace FetchFiles.Common;

using FetchFiles.Common.Internals;

public interface ICommand : IAsyncDisposable
{
    void ParseArgs(ParseArgs args, List<string> errors);
    Task Run();
}
