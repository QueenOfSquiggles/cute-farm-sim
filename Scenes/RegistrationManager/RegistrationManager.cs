using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using queen.error;
using queen.extension;

public partial class RegistrationManager : Node
{

    public static RegistrationManager Instance = null;

    // TODO create a unique dictionary for each registered type
    public Dictionary<string, Plot> Plots { get; private set; } = new();
    public Dictionary<string, WorldEntity> Entities { get; private set; } = new();

    private const string REGISTRY_PATH_PLOTS = "res://Game/Registries/Plots/";
    private const string REGISTRY_PATH_ENTITIES = "res://Game/Registries/Entities/";

    public override void _Ready()
    {
        if (!this.EnsureSingleton(ref Instance)) return;
        Print.Debug("[Registration Manager] - Begin registration");
        Plots = LoadRegistry<Plot>(REGISTRY_PATH_PLOTS, (Plot plot) => plot.ID, nameof(Plot));
        Entities = LoadRegistry<WorldEntity>(REGISTRY_PATH_ENTITIES, (WorldEntity we) => we.ID, nameof(WorldEntity));
        Print.Debug("[Registration Manager] - End registration");
    }

    private delegate string GetIDFor<T>(T obj) where T : Resource; // Delegates for callbacks are frickin awesome. Eat your heart out GDScript!
    private Dictionary<string, T> LoadRegistry<T>(string root_dir, GetIDFor<T> idGenCallback, string label = "unlabeled") where T : Resource
    {
        var registry = new Dictionary<string, T>();
        var dir = DirAccess.Open(root_dir);
        if (dir is null)
        {
            Print.Debug($"Failed to find root path for resource [{label}], expected path : {root_dir}");
            return registry; // empty registry
        }
        var files = GetAllFilesRecursive(dir);

        foreach (string f in files)
        {
            var temp = GD.Load<T>(f);
            if (temp is not null)
            {
                Print.Debug($"Found registry resource [{label}] : {f}");
                registry.Add(idGenCallback(temp), temp);
            }
            else
            {
                Print.Debug($"file '{f}' is not valid for type [{label}]");
            }
        }
        PrintRegistry(registry);
        return registry;
    }

    private void PrintRegistry<T>(Dictionary<string, T> reg, string label = "unlabeled") where T : Resource
    {
        Print.Debug($"----['{label}' Registry ({reg.Count} elements) ]----");
        foreach (var pair in reg)
        {
            Print.Debug($"] '{pair.Key}' : {pair.Value}");
        }
        Print.Debug($"----[End Registry ]----");
    }

    private List<string> GetAllFilesRecursive(DirAccess dir)
    {
        var files = new List<string>();
        foreach (var f in dir.GetFiles())
        {
            files.Add(dir.GetCurrentDir().PathJoin(f));
        }

        foreach (var d in dir.GetDirectories())
        {
            if (d.StartsWith("_")) continue; // allow special hidden folders
            var subdir = DirAccess.Open(dir.GetCurrentDir().PathJoin(d));
            if (subdir is null) continue;
            files.AddRange(GetAllFilesRecursive(subdir));
        }
        return files;
    }

}