
using FetchFiles.Common;
using FetchFiles.Common.Internals;
using Newtonsoft.Json;
using System.Text;

var context = new FetchFilesContext();
context.Commands = new Dictionary<string, Func<FetchFilesContext, ICommand>>();
context.Commands.Add("unit-list-files", c => new UnitListFilesCommand(c));
context.Commands.Add("sync", c => new OneShotRun(c));
context.UnitTypes = new Dictionary<string, Func<Unit,IUnitProvider>>();
context.UnitTypes.Add("Local", u => new LocalUnitProvider(u));
context.UnitTypes.Add("SFTP", u => new SftpUnit(u));
context.StoreTypes = new Dictionary<string, Func<Store,IStoreProvider>>();
context.StoreTypes.Add("Local", u => new LocalStoreProvider(u));

var parser = new ParseArgs(args);
string arg;
var errors = new List<string>();
ICommand? command = null, helpCommand = null;
while (parser.MoveNext())
{
    if (parser.Is(arg = "--help", "-h"))
    {
        helpCommand = new ShowUsage(context);
    }
    else if (parser.Is(arg = "--config", "-c"))
    {
        if (parser.Has(1))
        {
            parser.MoveNext();
            var configPath = parser.Current;
            
            try
            {
                using var file = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(file, Encoding.UTF8);
                using var jsonReader = new JsonTextReader(reader);
                var serializer = new JsonSerializer();
                context.Config = serializer.Deserialize<Configuration>(jsonReader) ?? throw new InvalidOperationException("Configuration tree is null");

            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
        }
        else
        {
            errors.Add("Option " + arg + " requires a value");
        }
    }
    else if (command == null && context.Commands.ContainsKey(arg = parser.Current))
    {
        if (context.Config == null)
        {
            errors.Add("No config file path specified");
        }

        command = context.Commands[arg](context);
        
        command.ParseArgs(parser, errors);
    }
    else
    {
        errors.Add("Unknown command " + parser.Current + "");
    }
}

if (command == null)
{
    errors.Add("One command must be specified");
}

if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine(error);
    }

    return 1;
}

await (helpCommand ?? command!).Run();

await command!.DisposeAsync();

return 0;