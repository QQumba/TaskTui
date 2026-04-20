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
        var bufferStart = Console.CursorLeft - buffer.Length;

        while (true)
        {
            var bufferLeft = Console.CursorLeft - bufferStart;
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
                case ConsoleKey.LeftArrow:
                    if (Console.CursorLeft > bufferStart)
                    {
                        Console.CursorLeft--;
                    }

                    break;
                case ConsoleKey.RightArrow:
                    if (Console.CursorLeft < bufferStart + buffer.Length)
                    {
                        Console.CursorLeft++;
                    }

                    break;

                case ConsoleKey.Home:
                    Console.CursorLeft = bufferStart;
                    break;
                case ConsoleKey.End:
                    Console.CursorLeft = bufferStart + buffer.Length;
                    break;
                case ConsoleKey.Backspace:
                    if (buffer.Length > 0 && bufferLeft > 0)
                    {
                        buffer.Remove(bufferLeft - 1, 1);
                        bufferLeft--;
                        Console.CursorLeft--;

                        Console.Write(new string(' ', buffer.Length - bufferLeft + 1));
                        Console.CursorLeft -= buffer.Length - bufferLeft + 1;
                        Console.Write(buffer.ToString(bufferLeft, buffer.Length - bufferLeft));

                        Console.CursorLeft -= buffer.Length - bufferLeft;
                    }

                    break;
                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        buffer.Insert(bufferLeft, key.KeyChar);
                        bufferLeft++;

                        Console.Write(buffer.ToString(bufferLeft - 1, buffer.Length - bufferLeft + 1));
                        Console.CursorLeft -= buffer.Length - bufferLeft;
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