using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TelegramServer.Common;
using Volo.Abp.DependencyInjection;

namespace TelegramServer.Verifier;

public interface ITelegramTokenProvider
{
    string LoadToken();
}

public class TelegramTokenProvider : ITelegramTokenProvider, ISingletonDependency
{
    private readonly ILogger<TelegramTokenProvider> _logger;

    public TelegramTokenProvider(ILogger<TelegramTokenProvider> logger)
    {
        _logger = logger;
    }

    public string LoadToken()
    {
        _logger.LogInformation("Wait for the input of the token....");
        Task.Delay(1000);
        Console.WriteLine();

        Console.WriteLine("Enter the telegram token and press enter");
        string token = null;
        while ((token = InputAndCheckKey()).IsNullOrWhiteSpace())
        {
        }

        Console.WriteLine("Finished....");
        Console.WriteLine();
        return token;
    }

    private string InputAndCheckKey()
    {
        try
        {
            Console.Write("The token is: ");
            var key = ConsoleHelper.ReadKey();
            var showStr = string.Concat(key.AsSpan(0, 2), new string('*', key.Length - 4), key.AsSpan(key.Length - 2));
            Console.WriteLine($"The input token is: {showStr}, continue with: y, re-enter: n.");
            var confirm = Console.ReadLine();
            if (confirm?.ToLower() != "y")
            {
                return null;
            }

            return key;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed!");
            return null;
        }
    }
}