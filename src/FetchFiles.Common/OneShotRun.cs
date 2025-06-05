
namespace FetchFiles.Common;

using FetchFiles.Common.Internals;
using System;

public sealed class OneShotRun : ICommand
{
    private readonly FetchFilesContext context;
    private readonly Configuration config;

    public OneShotRun(FetchFilesContext context)
    {
        this.context = context;
        this.config = context.Config;
    }

    public void ParseArgs(ParseArgs args, List<string> errors)
    {
        while (args.MoveNext())
        {
            errors.Add("Unknown option " + args.Current);
        }
    }

    public async Task Run()
    {
        if (this.config.Units is null || this.config.Units.Count == 0)
        {
            return;
        }

        if (this.config.Stores is null || this.config.Stores.Count == 0)
        {
            return;
        }

        Console.WriteLine($"Preparing stores... ");
        var stores = new List<IStoreProvider>();
        foreach (var store in this.config.Stores)
        {
            store.Value.Name = store.Key;
            if (store.Value.Type == null || !this.context.StoreTypes.ContainsKey(store.Value.Type))
            {
                throw new InvalidOperationException($"Store type {store.Value.Type} not supported");
            }

            var provider = this.context.StoreTypes[store.Value.Type](store.Value);
            stores.Add(provider);
            await provider.Initialize();
        }

        Console.WriteLine($"Preparing tasks... ");
        var tasks = new List<Task>();
        foreach (var unit in this.context.EnumerateUnits())
        {
            if (unit.Type == null || !this.context.UnitTypes.ContainsKey(unit.Type))
            {
                throw new InvalidOperationException($"Unit type {unit.Type} not supported");
            }

            var unitProvider = this.context.UnitTypes[unit.Type](unit);
            await unitProvider.Initialize();
            tasks.Add(this.ProcessUnit(unit, unitProvider, stores)
               .ContinueWith(async _ => await unitProvider.DisposeAsync()));
        }

        Console.WriteLine($"Preparing tasks... done. Now waiting for tasks to end. ");
        Console.WriteLine($"Now waiting for tasks to end. ");
        await Task.WhenAll(tasks);

        Console.WriteLine($"Now waiting for tasks to end. done.");
        foreach (var store in stores)
        {
            await store.DisposeAsync();
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private async Task ProcessUnit(Unit unit, IUnitProvider provider, List<IStoreProvider> stores)
    {
        var tasks = new List<Task>();
        await foreach (var file in provider.ListFiles())
        {
            foreach (var store in stores)
            {
                if (await store.Accepts(file))
                {
                    bool process = false;
                    var existing = await store.GetFileInfo(file.Unit, file.FileName);
                    if (existing.Exists && existing.State != null)
                    {
                        if (file.Length != existing.State.Length)
                        {
                            Console.WriteLine($"{file.ToShortString()} -> {store.ToShortString()}: overwrite because file length is different");
                            process = true;
                        }
                        else if (file.LastWriteTimeUtc != existing.State.LastWriteTimeUtc)
                        {
                            Console.WriteLine($"{file.ToShortString()} -> {store.ToShortString()}: overwrite because file date is different");
                            process = true;
                        }
                        else
                        {
                            Console.WriteLine($"{file.ToShortString()} -> {store.ToShortString()}: skip      because file is equal    ");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{file.ToShortString()} -> {store.ToShortString()}: fetch     because file is absent   ");
                        process = true;
                    }

                    if (process)
                    {
                        Console.WriteLine($"{file.ToShortString()} -> {store.ToShortString()}: fetch start");
                        var downloadTask = store.Store(file, provider).ContinueWith(_ =>
                        {
                            Console.WriteLine($"{file.ToShortString()} -> {store.ToShortString()}: fetch end");
                        });
                        tasks.Add(downloadTask);
                        await downloadTask;
                    }
                }
                else
                {
                    Console.WriteLine($"{file.ToShortString()} -> {store.ToShortString()}: file is not accepted");
                }
            }
        }
        
        await Task.WhenAll(tasks);
    }
}