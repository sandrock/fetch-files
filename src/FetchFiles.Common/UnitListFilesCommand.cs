using FetchFiles.Common;
using FetchFiles.Common.Internals;

public sealed class UnitListFilesCommand(FetchFilesContext context) : ICommand
{
    private readonly FetchFilesContext context = context;

    string? unitName = null;

    public void ParseArgs(ParseArgs args, List<string> errors)
    {
        while (args.MoveNext())
        {
            if (unitName == null)
            {
                unitName = args.Current;
            }
            else
            {
                errors.Add("Unknown option " + args.Current);
            }
        }
    }

    public async Task Run()
    {
        if (this.context.Config.Units is null || this.context.Config.Units.Count == 0)
        {
            return;
        }

        if (this.unitName == null)
        {
            return;
        }

        var unitConfig = this.context.Config.Units[this.unitName!];
        unitConfig.Name = this.unitName;
        if (unitConfig.Type == null || !this.context.UnitTypes.ContainsKey(unitConfig.Type))
        {
            throw new InvalidOperationException($"Unit type {unitConfig.Type} not supported");
        }

        await using var provider = this.context.UnitTypes[unitConfig.Type](unitConfig);
        await provider.Initialize();

        await foreach (var file in provider.ListFiles())
        {
            Console.Out.WriteLine(file);
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
