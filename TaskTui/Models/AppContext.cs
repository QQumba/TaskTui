namespace TaskTui.Models;

public class AppContext
{
    public RunnableTask? Task { get; set; }
    public Dictionary<string, string?>? Variables { get; set; }
}