namespace TaskTui.Models;

public class TaskDefinition
{
    public string? Desc { get; set; }
    public Dictionary<string, string>? Vars { get; set; }
    public RequiresDefinition? Requires { get; set; }
}