using TaskTui.Models;

namespace TaskTui;

public static class CacheExtensions
{
    public static List<RunnableTask> GetSortedTasks(this ICache cache, List<RunnableTask> tasks)
    {
        // filter out duplicates and tasks that don't exist
        var filteredLatestTasks = cache.State.LatestTasks
            .Distinct() 
            .Where(taskName => tasks.Any(x => x.Name == taskName)) 
            .ToList();

        // get latest tasks first and append other tasks, that don't exist in cache
        var sortedTasks = filteredLatestTasks
            .Select(x => tasks.First(t => t.Name == x))
            .Concat(tasks.Where(t => !filteredLatestTasks.Contains(t.Name)))
            .ToList();

        cache.State.LatestTasks = filteredLatestTasks;
        return sortedTasks;
    }
    
    public static void AddTask(this ICache cache, string taskName)
    {
        
        var latestTasks = new LinkedList<string>(cache.State.LatestTasks);
        
        latestTasks.Remove(taskName);
        latestTasks.AddFirst(taskName);
        
        const int maxLatestTasks = 10;
        if (latestTasks.Count > maxLatestTasks)
        {
            latestTasks.RemoveLast();
        }

        cache.State.LatestTasks = latestTasks.ToList();
    }

    public static void AddVariable(this ICache cache, string name, string? value)
    {
        if (value is null)
        {
            return;
        }
        
        cache.State.Vars[name] = value;
    }
}