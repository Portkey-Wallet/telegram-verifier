using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TelegramServer.Common;
using Volo.Abp.DependencyInjection;

namespace TelegramServer.Auth;

public interface IJwtTokenPrivateKeyProvider
{
    string LoadPrivateKey();
}

public class JwtTokenPrivateKeyProvider : IJwtTokenPrivateKeyProvider, ISingletonDependency
{
    private readonly ILogger<JwtTokenPrivateKeyProvider> _logger;

    public JwtTokenPrivateKeyProvider(ILogger<JwtTokenPrivateKeyProvider> logger)
    {
        _logger = logger;
    }

    public string LoadPrivateKey()
    {
        _logger.LogInformation("Wait for the input of the key....");
        Task.Delay(1000);
        Console.WriteLine();

        Console.WriteLine("Enter the key used to generate the Jwt Token and press enter");
        string key = null;
        while ((key = InputAndCheckKey()).IsNullOrWhiteSpace())
        {
        }

        Console.WriteLine("\nFinished....");
        Console.WriteLine();
        return key;
    }

    private string InputAndCheckKey()
    {
        try
        {
            Console.Write("The key is: ");
            var key = ConsoleHelper.ReadKey();
            key = key.Replace("\\n", "\n");
            var privateKey = Convert.FromBase64String(key);
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportPkcs8PrivateKey(privateKey, out _);
            var rsaParameters = rsa.ExportParameters(false);
            _ = Base64UrlEncoder.Encode(rsaParameters.Modulus);
            _ = Base64UrlEncoder.Encode(rsaParameters.Exponent);
            return key;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed!");
            return null;
        }
    }
}