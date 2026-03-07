namespace TaskTui.Models;

public record RunnableTask(string Name, string? Description, params Variable[] Variables);