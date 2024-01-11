using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AElf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelegramServer.Common;
using TelegramServer.Common.Dtos;
using TelegramServer.Verifier.Options;
using Volo.Abp.DependencyInjection;

namespace TelegramServer.Verifier;

public interface ITelegramVerifyProvider
{
    Task<bool> ValidateTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);
}

public class TelegramVerifyProvider : ISingletonDependency, ITelegramVerifyProvider
{
    private ILogger<TelegramVerifyProvider> _logger;
    private readonly TelegramAuthOptions _telegramAuthOptions;
    private string _token;

    public TelegramVerifyProvider(ILogger<TelegramVerifyProvider> logger,
        IOptionsSnapshot<TelegramAuthOptions> telegramAuthOptions)
    {
        _logger = logger;
        _telegramAuthOptions = telegramAuthOptions.Value;
        LoadToken();
    }

    private void LoadToken()
    {
        _logger.LogInformation("Wait for the input of the token....");
        Task.Delay(1000);
        Console.WriteLine();

        Console.WriteLine("Enter the telegram token and press enter");
        while (!InputAndCheckKey())
        {
        }

        Console.WriteLine("Finished....");
        Console.WriteLine();
    }

    private bool InputAndCheckKey()
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
                return false;
            }

            _token = key;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed!");
            return false;
        }
    }


    public async Task<bool> ValidateTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto)
    {
        if (telegramAuthDataDto.Hash.IsNullOrWhiteSpace() || telegramAuthDataDto.Id.IsNullOrWhiteSpace())
        {
            _logger.LogError("hash/id parameter in the telegram callback is null. id={0}", telegramAuthDataDto.Id);
            return false;
        }

        var dataCheckString = GetDataCheckString(telegramAuthDataDto);
        var localHash = await GenerateTelegramHashAsync(_token, dataCheckString);
        if (!localHash.Equals(telegramAuthDataDto.Hash))
        {
            _logger.LogError("verification of the telegram information has failed. id={0}", telegramAuthDataDto.Id);
            return false;
        }

        if (!telegramAuthDataDto.AuthDate.IsNullOrWhiteSpace())
        {
            //validate auth date
            var expiredUnixTimestamp = (long)DateTime.UtcNow.AddSeconds(-_telegramAuthOptions.Expire)
                .Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var authDate = long.Parse(telegramAuthDataDto.AuthDate);
            if (authDate < expiredUnixTimestamp)
            {
                _logger.LogError("verification of the telegram information has failed, login timeout. id={0}",
                    telegramAuthDataDto.Id);
                return false;
            }
        }

        return true;
    }

    private static string GetDataCheckString(TelegramAuthDataDto telegramAuthDataDto)
    {
        Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        if (!telegramAuthDataDto.Id.IsNullOrWhiteSpace())
        {
            keyValuePairs.Add("id", telegramAuthDataDto.Id);
        }

        if (telegramAuthDataDto.UserName != null)
        {
            keyValuePairs.Add("username", telegramAuthDataDto.UserName);
        }

        if (telegramAuthDataDto.AuthDate != null)
        {
            keyValuePairs.Add("auth_date", telegramAuthDataDto.AuthDate);
        }

        if (telegramAuthDataDto.FirstName != null)
        {
            keyValuePairs.Add("first_name", telegramAuthDataDto.FirstName);
        }

        if (telegramAuthDataDto.LastName != null)
        {
            keyValuePairs.Add("last_name", telegramAuthDataDto.LastName);
        }

        if (telegramAuthDataDto.PhotoUrl != null)
        {
            keyValuePairs.Add("photo_url", telegramAuthDataDto.PhotoUrl);
        }

        var sortedByKey = keyValuePairs.Keys.OrderBy(k => k);
        var sb = new StringBuilder();
        foreach (var key in sortedByKey)
        {
            sb.AppendLine($"{key}={keyValuePairs[key]}");
        }

        sb.Length = sb.Length - 1;
        return sb.ToString();
    }

    private static Task<string> GenerateTelegramHashAsync(string token, string dataCheckString)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var dataCheckStringBytes = Encoding.UTF8.GetBytes(dataCheckString);
        var computeFrom = HashHelper.ComputeFrom(tokenBytes).ToByteArray();

        using var hmac = new HMACSHA256(computeFrom);
        var hashBytes = hmac.ComputeHash(dataCheckStringBytes);
        return Task.FromResult(hashBytes.ToHex());
    }
}