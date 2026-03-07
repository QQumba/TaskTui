namespace TaskTui.Models;

public class TaskFile
{
    public Dictionary<string, string>? Vars { get; set; }
    public Dictionary<string, TaskDefinition> Tasks { get; set; }
}