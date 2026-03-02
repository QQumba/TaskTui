using System.Diagnostics;
using Spectre.Console;

namespace TaskTui;

public static class Steps
{
    public static AppState PromptTask(AppContext ctx, ICollection<RunnableTask> tasks)
    {
        AnsiConsole.Clear();
        
        var task = AnsiConsole.Prompt(
            new SelectionPrompt<RunnableTask>()
                .Title("[green]Select task[/]")
                .EnableSearch()
                .UseConverter(t => t.Name)
                .AddChoices(tasks));
        
        ctx.Task = task;
        return AppState.SelectVariables;
    }
    
    public static AppState PromptVariables(AppContext ctx)
    {
        if (ctx.Task is null)
        {
            throw new InvalidOperationException("No task selected");
        }
        
        ShowTaskInfo(ctx.Task);
        
        var variables = new Dictionary<string, string?>();
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
            
            variables[variable.Name] = value;
        }
        
        if (variables.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[grey]Press Enter to run the task...[/]");
            Console.ReadLine();
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
}