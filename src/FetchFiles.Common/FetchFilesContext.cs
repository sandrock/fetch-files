
namespace FetchFiles.Common;

using System;

public sealed class FetchFilesContext
{
    public Configuration Config { get; set; } = null!;
    public Dictionary<string, Func<FetchFilesContext, ICommand>> Commands { get; set; } = null!;
    public Dictionary<string, Func<Unit, IUnitProvider>> UnitTypes { get; set; } = null!;
    public Dictionary<string, Func<Store, IStoreProvider>> StoreTypes { get; set; } = null!;

    public IEnumerable<Unit> EnumerateUnits()
    {
        if (this.Config?.Units == null || this.Config.Units.Count == 0)
        {
            yield break;
        }

        foreach (var unit in this.Config.Units)
        {
            unit.Value.Name = unit.Key;
            yield return unit.Value;
        }
    }
}
