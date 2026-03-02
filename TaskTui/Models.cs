namespace TaskTui;

public class TaskFile
{
    public Dictionary<string, string>? Vars { get; set; }
    public Dictionary<string, TaskDefinition> Tasks { get; set; }
}

public class TaskDefinition
{
    public string? Desc { get; set; }
    public Dictionary<string, string>? Vars { get; set; }
    public RequiresDefinition? Requires { get; set; }
}

public class RequiresDefinition
{
    public List<string>? Vars { get; set; }
}

public record RunnableTask(string Name, string? Description, params Variable[] Variables);

public class Variable
{
    public string Name { get; set; }
    public string? DefaultValue { get; set; }
}