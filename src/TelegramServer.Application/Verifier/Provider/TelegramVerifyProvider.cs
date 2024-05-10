using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TelegramServer.Common;
using TelegramServer.Common.Dtos;
using TelegramServer.Verifier.Options;
using TelegramServer.Verifier.Provider;
using Volo.Abp.DependencyInjection;

namespace TelegramServer.Verifier;

public interface ITelegramVerifyProvider
{
    Task<string> GenerateHashAsync(TelegramAuthDataDto telegramAuthDataDto);
    Task<bool> ValidateTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);

    Task<bool> ValidateTelegramDataAsync(IDictionary<string, string> data,
        Func<string, string, string> generateTelegramHash);
}

public class TelegramVerifyProvider : ISingletonDependency, ITelegramVerifyProvider
{
    private ILogger<TelegramVerifyProvider> _logger;
    private readonly TelegramAuthOptions _telegramAuthOptions;
    private JObject _token;
    private readonly string _defaultPortkeyRobotId;

    public TelegramVerifyProvider(ILogger<TelegramVerifyProvider> logger,
        IOptions<TelegramAuthOptions> telegramAuthOptions, ITelegramTokenProvider telegramTokenProvider)
    {
        _logger = logger;
        _telegramAuthOptions = telegramAuthOptions.Value;
        _token = telegramTokenProvider.LoadToken();
        _defaultPortkeyRobotId = "portkey-tg-robot";
    }

    public Task<string> GenerateHashAsync(TelegramAuthDataDto telegramAuthDataDto)
    {
        var dataCheckString = GetDataCheckString(telegramAuthDataDto);
        return Task.FromResult(GenerateTelegramDataHash.AuthDataHash(ExtractTokenFromLoadToken(telegramAuthDataDto.BotId), dataCheckString));
    }

    public Task<bool> ValidateTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto)
    {
        if (telegramAuthDataDto.Hash.IsNullOrWhiteSpace() || telegramAuthDataDto.Id.IsNullOrWhiteSpace())
        {
            _logger.LogError("hash/id parameter in the telegram callback is null. id={0}", telegramAuthDataDto.Id);
            return Task.FromResult(false);
        }

        var dataCheckString = GetDataCheckString(telegramAuthDataDto);
        //todo delete before online
        _logger.LogInformation("verification of the telegram information botId={0}", telegramAuthDataDto.BotId);
        _logger.LogInformation("verification of the telegram information token in JSON format={0}", _token.ToString());
        string extractedToken = ExtractTokenFromLoadToken(telegramAuthDataDto.BotId);
        
        var localHash = GenerateTelegramDataHash.AuthDataHash(extractedToken, dataCheckString);
        if (!localHash.Equals(telegramAuthDataDto.Hash))
        {
            _logger.LogError("verification of the telegram information has failed. id={0}", telegramAuthDataDto.Id);
            return Task.FromResult(false);
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
                return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    private string ExtractTokenFromLoadToken(string botId)
    {
        JToken loadedToken;
        //前端不传机器人id，默认取portkey的token
        if (botId.IsNullOrEmpty())
        {
            loadedToken = _token.GetValue(_defaultPortkeyRobotId);
            if (loadedToken == null)
            {
                _logger.LogError("load default portkey token error, robotId={0}", botId);
                return string.Empty;
            }
            return loadedToken.Value<string>();
        }
        loadedToken = _token.GetValue(botId);
        if (loadedToken == null)
        {
            _logger.LogError("load tg token error, robotId={0}", botId);
            return string.Empty;
        }
        return loadedToken.Value<string>();
    }

    public Task<bool> ValidateTelegramDataAsync(IDictionary<string, string> data,
        Func<string, string, string> generateTelegramHash)
    {
        if (data.IsNullOrEmpty() || !data.ContainsKey(CommonConstants.RequestParameterNameHash) ||
            data[CommonConstants.RequestParameterNameHash].IsNullOrWhiteSpace())
        {
            _logger.LogError("telegramData or telegramData[hash] is empty");
            return Task.FromResult(false);
            ;
        }

        var dataCheckString = GetDataCheckString(data);
        //todo remove before online
        _logger.LogInformation("telegram verify ValidateTelegramDataAsync data={0}", JsonConvert.SerializeObject(data));
        string botIdFromData = data.ContainsKey(CommonConstants.RequestParameterRobotId) ? data[CommonConstants.RequestParameterRobotId] : string.Empty;
        var localHash = generateTelegramHash(ExtractTokenFromLoadToken(botIdFromData), dataCheckString);
        if (!localHash.Equals(data[CommonConstants.RequestParameterNameHash]))
        {
            _logger.LogDebug("verification of the telegram information has failed. data={0}",
                JsonConvert.SerializeObject(data));
            return Task.FromResult(false);
        }

        if (data.ContainsKey(CommonConstants.RequestParameterNameAuthDate) &&
            !data[CommonConstants.RequestParameterNameAuthDate].IsNullOrWhiteSpace())
        {
            var expiredUnixTimestamp = (long)DateTime.UtcNow.AddSeconds(-_telegramAuthOptions.Expire)
                .Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var authDate = long.Parse(data[CommonConstants.RequestParameterNameAuthDate]);
            if (authDate < expiredUnixTimestamp)
            {
                _logger.LogDebug("verification of the telegram information has failed, login timeout. data={0}",
                    JsonConvert.SerializeObject(data));
                return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    private static string GetDataCheckString(TelegramAuthDataDto telegramAuthDataDto)
    {
        var keyValuePairs = new Dictionary<string, string>();
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

        return GetDataCheckString(keyValuePairs);
    }

    private static string GetDataCheckString(IDictionary<string, string> data)
    {
        var sortedByKey = data.Keys.OrderBy(k => k);
        var sb = new StringBuilder();
        foreach (var key in sortedByKey)
        {
            if (key == CommonConstants.RequestParameterNameHash || CommonConstants.RequestParameterRobotId.Equals(key))
            {
                continue;
            }

            sb.AppendLine($"{key}={data[key]}");
        }

        sb.Length -= 1;
        return sb.ToString();
    }
}