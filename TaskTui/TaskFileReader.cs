using Spectre.Console;
using TaskTui.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TaskTui;

public class TaskFileReader
{
    private static readonly string[] TaskFileNames =
    [
        "Taskfile.yml",
        "taskfile.yml",
        "Taskfile.yaml",
        "taskfile.yaml",
        "Taskfile.dist.yml",
        "taskfile.dist.yml",
        "Taskfile.dist.yaml",
        "taskfile.dist.yaml",
    ];

    public static async Task<TaskFile> ReadTaskFile()
    {
        var taskFile = TaskFileNames.FirstOrDefault(File.Exists);
        if (taskFile is null)
        {
            AnsiConsole.MarkupLine("[red]No taskfile was found in the current directory. [/]");
            Environment.Exit(1);
            return null;
        }

        var yaml = await File.ReadAllTextAsync(taskFile);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<TaskFile>(yaml);
    }

    public static List<RunnableTask> GetTasks(TaskFile taskFile)
    {
        var fileVariables = taskFile.Vars ?? new Dictionary<string, string>();

        var tasks = taskFile.Tasks.Select(kv =>
        {
            var requiredVariables = kv.Value.Requires?.Vars ?? [];
            var taskVariables = kv.Value.Vars ?? new Dictionary<string, string>();

            var variables = requiredVariables.Select(name => new Variable { Name = name }).ToArray();
            foreach (var v in variables)
            {
                // if task level vars not found, search in file level vars
                if (!taskVariables.TryGetValue(v.Name, out var defaultValue))
                {
                    fileVariables.TryGetValue(v.Name, out defaultValue);
                }

                v.DefaultValue = defaultValue;
            }

            return new RunnableTask(kv.Key, kv.Value.Desc, variables);
        }).ToList();

        return tasks;
    }
}