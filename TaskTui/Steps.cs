using System.Diagnostics;
using Spectre.Console;
using TaskTui.Models;
using AppContext = TaskTui.Models.AppContext;

namespace TaskTui;

public static class Steps
{
    public static AppState PromptTask(AppContext ctx, ICollection<RunnableTask> tasks)
    {
        AnsiConsole.Clear();
        
        using var cache = new Cache();
        var sortedTasks = GetSortedTasks(tasks, cache.LatestRunTasks);
        
        var maxNameWidth = tasks.Max(t => t.Name.Length);
        maxNameWidth = Math.Min(maxNameWidth, 30);
        
        var task = AnsiConsole.Prompt(
            new SelectionPrompt<RunnableTask>()
                .Title("[green]Select task[/]")
                .EnableSearch()
                .UseConverter(t =>
                {
                    var paddedName = t.Name.PadRight(maxNameWidth);
                    
                    var displayedText = $"[green]{paddedName}[/]";
                    if (t.Description != null)
                    {
                        displayedText += $"    [grey]{Markup.Escape(t.Description)}[/]";
                    }
                    
                    return displayedText;
                })
                .AddChoices(sortedTasks));
        
        cache.AddTask(task.Name);
        ctx.Task = task;
        return AppState.SelectVariables;
    }
    
    public static AppState PromptVariables(AppContext ctx)
    {
        if (ctx.Task is null)
        {
            throw new InvalidOperationException("No task selected");
        }

        var variables = new Dictionary<string, string?>();
        if (ctx.Task.Variables.Length == 0)
        {
            ctx.Variables = variables;
            return AppState.RunTask;
        }

        using var cache = new Cache();
        foreach (var variable in ctx.Task.Variables)
        {
            if (cache.Variables.TryGetValue(variable.Name, out var cachedVar))
            {
                variable.DefaultValue = cachedVar;
            }
        }
        
        ShowTaskInfo(ctx.Task);
        
        foreach (var variable in ctx.Task.Variables)
        {
            
            CancellableTextPrompt prompt;
            
            if (variable.DefaultValue is null)
            {
                prompt = new CancellableTextPrompt(variable.Name); 
            }
            else
            {
                prompt = new CancellableTextPrompt(variable.Name)
                    .DefaultValue(variable.DefaultValue)
                    .AllowEmpty();
            }

            var (promptResult, value) = prompt.Show();
            if (promptResult == PromptResult.Cancel)
            {
                return AppState.SelectTask;
            }

            cache.AddVariable(variable.Name, value);
            variables[variable.Name] = value;
        }

        ctx.Variables = variables;
        return AppState.RunTask;
    }

    public static async Task<AppState> RunTask(AppContext ctx)
    {
        if (ctx.Task is null || ctx.Variables is null)
        {
            throw new InvalidOperationException("Task or variables not set");
        }
        
        AnsiConsole.Clear();

        var arguments = ctx.Task.Name;
        foreach (var kv in ctx.Variables.Where(kv => kv.Value != null))
        {
            arguments += $" {kv.Key}={kv.Value}";
        }
        
        AnsiConsole.MarkupLine($"[yellow]Running:[/] {arguments}\n");

        var psi = new ProcessStartInfo
        {
            FileName = "task",
            Arguments = arguments,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
        };

        using var process = new Process();
        process.StartInfo = psi;

        process.Start();
        await process.WaitForExitAsync();
        
        AnsiConsole.MarkupLine("\n[grey]Press Enter to return to task list...[/]");
        Console.ReadLine();
        
        return AppState.SelectTask;
    }

    private static void ShowTaskInfo(RunnableTask task)
    {
        var infoPanel = new Panel(
            $"[green]Description:[/] {task.Description}\n" +
            $"[yellow]Variables:[/] {string.Join(", ", string.Join(", ", task.Variables.Select(v => {
                var variable = $"{v.Name}";
                if (v.DefaultValue != null) 
                {
                    variable += $" [green]({v.DefaultValue})[/]";
                }

                return variable;
            })))}")
        {
            Header = new PanelHeader(task.Name)
        };

        AnsiConsole.Write(infoPanel);
    }

    private static List<RunnableTask> GetSortedTasks(IEnumerable<RunnableTask> tasks, IEnumerable<string> lastRunTaskNames)
    {
        var lastRunTasks = lastRunTaskNames
            .Distinct()
            .Select(x => tasks.FirstOrDefault(t => t.Name == x))
            .Where(x => x != null)
            .ToList();

        var sortedTasks = new List<RunnableTask>(lastRunTasks!);
        sortedTasks.AddRange(tasks.Where(t => !lastRunTasks.Contains(t)));
        
        return sortedTasks;
    }
}