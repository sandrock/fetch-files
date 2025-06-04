
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

        var tasks = new List<Task>();
        foreach (var unit in this.context.EnumerateUnits())
        {
            var unitProvider = this.context.UnitTypes[unit.Type](unit);
            if (unit.Type == null || !this.context.StoreTypes.ContainsKey(unit.Type))
            {
                throw new InvalidOperationException($"Unit type {unit.Type} not supported");
            }

            await unitProvider.Initialize();
            tasks.Add(this.ProcessUnit(unit, unitProvider, stores));
        }

        await Task.WhenAll(tasks);
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
                    tasks.Add(store.Store(file, provider));
                }
            }
        }
        
        await Task.WhenAll(tasks);
    }
}