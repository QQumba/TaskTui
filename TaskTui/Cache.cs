using System.Text.Json;

namespace TaskTui;

public sealed class Cache : IDisposable
{
    private const string CacheFileName = "tasktui.cache.json";
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly CacheState _state = Load();
    
    public IReadOnlyList<string> LatestRunTasks => _state.LatestTasks;
    public IReadOnlyDictionary<string, string> Variables => _state.Vars;

    public void AddTask(string taskName)
    {
        var list = new LinkedList<string>(_state.LatestTasks);
        
        list.Remove(taskName);
        list.AddFirst(taskName);
        if (list.Count > 10)
        {
            list.RemoveLast();
        }

        _state.LatestTasks = list.ToList();
    }

    public void AddVariable(string name, string? value)
    {
        if (value is null)
        {
            return;
        }
        
        _state.Vars[name] = value;
    }
    
    private static CacheState Load()
    {
        if (!File.Exists(CacheFileName))
        {
            return new CacheState();
        }

        try
        {
            return JsonSerializer.Deserialize<CacheState>(File.ReadAllText(CacheFileName), Options) ?? new CacheState();
        }
        catch (Exception)
        {
            return new CacheState();
        }
    }
    
    private static void Save(CacheState state)
    {
        File.WriteAllText(CacheFileName, JsonSerializer.Serialize(state, Options));
    }

    public void Dispose()
    {
        Save(_state);
    }
}

public class CacheState
{
    public List<string> LatestTasks { get; set; } = [];
    public Dictionary<string, string> Vars { get; set; } = [];
}