using FetchFiles.Common;
using FetchFiles.Common.Internals;

public sealed class ShowUsage(FetchFilesContext context) : ICommand
{
    public void ParseArgs(ParseArgs args, List<string> errors)
    {
    }

    public Task Run()
    {
        Console.WriteLine("Usage: FetchFiles [-c|--config <config>] [-h|--help] <command> [command option]");
        Console.WriteLine();
        Console.Write("Available commands: ");
        var sep = String.Empty;
        foreach (var command in context.Commands)
        {
            Console.Write(sep);
            Console.Write(command.Key);
            sep = ", ";
        }
        
        Console.WriteLine();
        Console.WriteLine();
        return Task.CompletedTask;
    }
}
