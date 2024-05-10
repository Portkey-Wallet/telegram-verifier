using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TelegramServer.Common;
using Volo.Abp.DependencyInjection;

namespace TelegramServer.Verifier;

public interface ITelegramTokenProvider
{
    JObject LoadToken();
}

public class TelegramTokenProvider : ITelegramTokenProvider, ISingletonDependency
{
    private readonly ILogger<TelegramTokenProvider> _logger;

    public TelegramTokenProvider(ILogger<TelegramTokenProvider> logger)
    {
        _logger = logger;
    }

    public JObject LoadToken()
    {
        _logger.LogInformation("Wait for the input of the token....");
        Task.Delay(1000);
        Console.WriteLine();

        Console.WriteLine("Enter the telegram token in JSON format and press enter");
        Console.WriteLine("for example: {\"portkey-robot-id\":\"portkey-robot-token\",\"rebot-id\":\"rebot-token\"}");
        JObject token = null;
        while ((token = InputAndCheckKey()) == null)
        {
        }

        Console.WriteLine("Your input token in JSON format: {0}", token.ToString());
        Console.WriteLine("Finished....");
        Console.WriteLine();
        return token;
    }

    private JObject InputAndCheckKey()
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

            if (!IsValidJson(key))
            {
                return null;
            }
            return JObject.Parse(key);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed!");
            return null;
        }
    }
    
    private bool IsValidJson(string strInput)
    {
        if (string.IsNullOrWhiteSpace(strInput))
        {
            return false;
        }
        strInput = strInput.Trim();
        try
        {
            var obj = JsonConvert.DeserializeObject(strInput);
            return true;
        }
        catch
        {
            return false;
        }
    }
}