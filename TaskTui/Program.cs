using TaskTui;
using TaskTui.Models;
using static TaskTui.Steps;
using AppContext = TaskTui.Models.AppContext;

Console.WriteLine("Hello, World!");

var taskFile = await TaskFileReader.ReadTaskFile();
var tasks = TaskFileReader.GetTasks(taskFile);

var ctx = new AppContext();
var state = AppState.SelectTask;
while (state != AppState.Exit)
{
    state = state switch
    {
        AppState.SelectTask => PromptTask(ctx, tasks),
        AppState.SelectVariables => PromptVariables(ctx),
        AppState.RunTask => await RunTask(ctx),
        _ => AppState.Exit
    };
}