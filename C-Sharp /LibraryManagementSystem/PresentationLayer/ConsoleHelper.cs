using BusinessLayer.Exceptions;
using PresentationLayer.Logging;

namespace PresentationLayer;

public static class ConsoleHelper
{
    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n✔ {message}");
        Console.ResetColor();
        FileLogger.LogSuccess(message);
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n✘ {message}");
        Console.ResetColor();
        FileLogger.LogError(message);
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n  {message}");
        Console.ResetColor();
        FileLogger.LogInfo(message);
    }

    public static void PrintHeader(string title)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n{"─",-40}");
        Console.WriteLine($"  {title}");
        Console.WriteLine($"{"─",-40}");
        Console.ResetColor();
        FileLogger.LogSection(title);
    }

    public static string ReadInput(string prompt)
    {
        Console.Write($"  {prompt}: ");
        var value = Console.ReadLine()?.Trim() ?? string.Empty;
        FileLogger.LogInput(prompt, value);
        return value;
    }

    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            var raw = Console.ReadLine()?.Trim() ?? string.Empty;

            if (int.TryParse(raw, out int value))
            {
                FileLogger.LogInput(prompt, raw);
                return value;
            }

            PrintError("Invalid number. Please enter a valid integer.");
        }
    }

    public static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write($"  {prompt}: ");
            var raw = Console.ReadLine()?.Trim() ?? string.Empty;

            if (decimal.TryParse(raw, out decimal value))
            {
                FileLogger.LogInput(prompt, raw);
                return value;
            }

            PrintError("Invalid amount. Please enter a valid number.");
        }
    }

    public static void PrintTable(string header, string separator, List<string> rows)
    {
        Console.WriteLine(header);
        Console.WriteLine(separator);
        FileLogger.Log(header);
        FileLogger.Log(separator);

        foreach (var row in rows)
        {
            Console.WriteLine(row);
            FileLogger.Log(row);
        }
    }

    public static void HandleException(Exception ex)
    {
        if (ex is LibraryException or InvalidInputException)
        {
            PrintError(ex.Message);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  Unexpected Error: {ex.Message}");
            Console.ResetColor();
            FileLogger.LogError($"Unexpected Error: {ex.Message}");
        }
    }

    public static void Pause()
    {
        Console.WriteLine("\n  Press any key to continue...");
        FileLogger.LogSeparator();
        Console.ReadKey();
    }
}