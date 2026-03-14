using System.Text.Json;

namespace TaskTui;

public interface ICache
{
    public CacheState State { get; }
}

public sealed class Cache : ICache, IDisposable
{
    public const string DefaultCacheFileName = "tasktui.cache.json";
    
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly string _cacheFileName;

    public CacheState State { get; }

    public Cache() : this(DefaultCacheFileName)
    {
    }

    public Cache(string cacheFileName)
    {
        _cacheFileName = cacheFileName;
        State = Load();
    }

    private CacheState Load()
    {
        if (!File.Exists(_cacheFileName))
        {
            return new CacheState();
        }

        try
        {
            return JsonSerializer.Deserialize<CacheState>(File.ReadAllText(_cacheFileName), Options) ?? new CacheState();
        }
        catch (Exception)
        {
            return new CacheState();
        }
    }

    private void Save(CacheState state)
    {
        File.WriteAllText(_cacheFileName, JsonSerializer.Serialize(state, Options));
    }

    public void Dispose()
    {
        Save(State);
    }
}

public class CacheState
{
    public List<string> LatestTasks { get; set; } = [];
    public Dictionary<string, string> Vars { get; set; } = [];
}