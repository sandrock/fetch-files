
namespace FetchFiles.Common;

using FetchFiles.Common.Internals;

public interface ICommand
{
    void ParseArgs(ParseArgs args, List<string> errors);
    Task Run();
}
