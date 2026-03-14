using System.Text;
using Spectre.Console;

namespace TaskTui;

public enum PromptResult
{
    Value,
    Cancel
}

public class CancellableTextPrompt
{
    private readonly string _prompt;

    public string? DefaultValue { get; set; }

    public bool AllowEmpty { get; set; }

    public CancellableTextPrompt(string prompt)
    {
        _prompt = prompt;
    }

    public (PromptResult result, string? value) Show()
    {
        var buffer = new StringBuilder(DefaultValue ?? string.Empty);
        AnsiConsole.Markup($"{_prompt}: [green]{buffer}[/]");

        while (true)
        {
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return (PromptResult.Cancel, DefaultValue);

                case ConsoleKey.Enter:
                    if (buffer.Length == 0 && !AllowEmpty)
                    {
                        continue;
                    }

                    Console.WriteLine();
                    var value = buffer.Length == 0 ? DefaultValue : buffer.ToString();
                    return (PromptResult.Value, value);

                case ConsoleKey.Backspace:
                    if (buffer.Length > 0)
                    {
                        buffer.Remove(buffer.Length - 1, 1);
                        Console.CursorLeft--;
                        Console.Write(" ");
                        Console.CursorLeft--;
                    }

                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        buffer.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }

                    break;
            }
        }
    }
}

public static class CancellableTextPromptExtensions
{
    public static CancellableTextPrompt DefaultValue(this CancellableTextPrompt prompt, string defaultValue)
    {
        prompt.DefaultValue = defaultValue;
        return prompt;   
    }
    
    public static CancellableTextPrompt AllowEmpty(this CancellableTextPrompt prompt)
    {
        prompt.AllowEmpty = true;
        return prompt;   
    }
}