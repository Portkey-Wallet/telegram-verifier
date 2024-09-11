using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TelegramServer.Common;
using TelegramServer.Common.Dtos;
using TelegramServer.Entities.Es;
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

    string EncryptSecret(string plainText, string currentTimestamp, string botId);

    string DecryptSecret(string secret, string currentTimestamp, string botId);
}

public class TelegramVerifyProvider : ISingletonDependency, ITelegramVerifyProvider
{
    private const int AesKeyLength = 16;
    private ILogger<TelegramVerifyProvider> _logger;
    private readonly TelegramAuthOptions _telegramAuthOptions;
    private JObject _token;
    private readonly string _defaultPortkeyRobotId;
    private readonly INESTRepository<TelegramBotIndex, Guid> _telegramBotRepository;

    public TelegramVerifyProvider(ILogger<TelegramVerifyProvider> logger,
        IOptions<TelegramAuthOptions> telegramAuthOptions, ITelegramTokenProvider telegramTokenProvider,
        INESTRepository<TelegramBotIndex, Guid> telegramBotRepository)
    {
        _logger = logger;
        _telegramAuthOptions = telegramAuthOptions.Value;
        _token = telegramTokenProvider.LoadToken();
        _defaultPortkeyRobotId = "portkey-tg-robot";
        _telegramBotRepository = telegramBotRepository;
    }

    public async Task<string> GenerateHashAsync(TelegramAuthDataDto telegramAuthDataDto)
    {
        var dataCheckString = GetDataCheckString(telegramAuthDataDto);
        return GenerateTelegramDataHash.AuthDataHash(await ExtractTokenFromLoadToken(telegramAuthDataDto.BotId), dataCheckString);
    }

    public async Task<bool> ValidateTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto)
    {
        if (telegramAuthDataDto.Hash.IsNullOrWhiteSpace() || telegramAuthDataDto.Id.IsNullOrWhiteSpace())
        {
            _logger.LogError("hash/id parameter in the telegram callback is null. id={0}", telegramAuthDataDto.Id);
            return false;
        }

        var dataCheckString = GetDataCheckString(telegramAuthDataDto);
        string extractedToken = await ExtractTokenFromLoadToken(telegramAuthDataDto.BotId);
        
        var localHash = GenerateTelegramDataHash.AuthDataHash(extractedToken, dataCheckString);
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

    private async Task<string> ExtractTokenFromLoadToken(string botId)
    {
        JToken loadedToken;
        // if botId is null, get the default portkey's token
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
            var tokenFromEs = await GetTelegramBotTokenByBotId(botId);
            if (!tokenFromEs.IsNullOrEmpty())
            {
                return tokenFromEs;
            }
        }
        if (loadedToken == null)
        {
            _logger.LogError("load tg token error, robotId={0}", botId);
            return string.Empty;
        }
        return loadedToken.Value<string>();
    }
    
    [ItemCanBeNull]
    private async Task<string> GetTelegramBotTokenByBotId(string botId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<TelegramBotIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(f => f.BotId).Value(botId))
        };
        QueryContainer Filter(QueryContainerDescriptor<TelegramBotIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var telegramBotIndic = await _telegramBotRepository.GetListAsync(Filter);
        _logger.LogDebug("GetTelegramBotTokenByBotId botId:{0} result:{1}", botId, JsonConvert.SerializeObject(telegramBotIndic));
        if (telegramBotIndic == null || telegramBotIndic.Item2.IsNullOrEmpty())
        {
            return null;
        }
        var telegramBotIndex =  telegramBotIndic.Item2.FirstOrDefault(bot => bot.BotId.Equals(botId));
        return telegramBotIndex == null ? null : DecryptSecret(telegramBotIndex.Secret, telegramBotIndex.CreateTime.ToString(), telegramBotIndex.BotId);
    }

    public async Task<bool> ValidateTelegramDataAsync(IDictionary<string, string> data,
        Func<string, string, string> generateTelegramHash)
    {
        if (data.IsNullOrEmpty() || !data.ContainsKey(CommonConstants.RequestParameterNameHash) ||
            data[CommonConstants.RequestParameterNameHash].IsNullOrWhiteSpace())
        {
            _logger.LogError("telegramData or telegramData[hash] is empty");
            return false;
        }

        var dataCheckString = GetDataCheckString(data);
        var botIdFromData = data.TryGetValue(CommonConstants.RequestParameterRobotId, out var value) ? value : string.Empty;
        var botToken = await ExtractTokenFromLoadToken(botIdFromData);
        var localHash = generateTelegramHash(botToken, dataCheckString);
        if (!localHash.Equals(data[CommonConstants.RequestParameterNameHash]))
        {
            _logger.LogDebug("verification of the telegram information has failed. data={0}",
                JsonConvert.SerializeObject(data));
            return false;
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
                return false;
            }
        }

        return true;
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

    public string EncryptSecret(string plainText, string currentTimestamp, string botId)
    {
        GetKeyAndIv(currentTimestamp, botId, out var key, out var iv);
        var encrypt = AesEncryptionProvider.Encrypt(plainText, Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));
        return encrypt;
    }
    
    public string DecryptSecret(string secret, string currentTimestamp, string botId)
    {
        GetKeyAndIv(currentTimestamp, botId, out var key, out var iv);
        var decrypt = AesEncryptionProvider.Decrypt(secret, Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));
        return decrypt;
    }

    private static void GetKeyAndIv(string currentTimestamp, string botId, out string key, out string iv)
    {
        if (currentTimestamp.Length > AesKeyLength)
        {
            key = currentTimestamp.Substring(0, AesKeyLength);
            iv = currentTimestamp.Substring(currentTimestamp.Length - AesKeyLength);
        }
        else
        {
            key = string.Concat(currentTimestamp, botId.AsSpan(0, AesKeyLength - currentTimestamp.Length));
            iv = string.Concat(botId.AsSpan(0, AesKeyLength - currentTimestamp.Length), currentTimestamp);
        }
    }
}