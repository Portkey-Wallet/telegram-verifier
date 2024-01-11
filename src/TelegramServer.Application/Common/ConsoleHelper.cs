using System;

namespace CATelegramServer.Common;

public class ConsoleHelper
{
    public static string ReadKey()
    {
        var pwd = "";
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }

            if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
            {
                pwd = pwd[..^1];
            }
            else if (key.Key != ConsoleKey.Backspace)
            {
                pwd += key.KeyChar;
            }
        }

        return pwd;
    }
}