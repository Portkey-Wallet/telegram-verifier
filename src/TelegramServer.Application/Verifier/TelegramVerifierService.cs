using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using Newtonsoft.Json;
using TelegramServer.Common;
using TelegramServer.Common.Dtos;
using TelegramServer.Entities.Es;
using TelegramServer.Verifier.Dto;
using TelegramServer.Verifier.Provider;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace TelegramServer.Verifier;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class TelegramVerifierService : TelegramServerAppService, ITelegramVerifierService
{
    private const string Colon = ":";
    private readonly ITelegramVerifyProvider _telegramVerifyProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly INESTRepository<TelegramBotIndex, Guid> _telegramBotRepository;

    public TelegramVerifierService(ITelegramVerifyProvider telegramVerifyProvider, IObjectMapper objectMapper,
        INESTRepository<TelegramBotIndex, Guid> telegramBotRepository)
    {
        _telegramVerifyProvider = telegramVerifyProvider;
        _objectMapper = objectMapper;
        _telegramBotRepository = telegramBotRepository;
    }

    public async Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(
        TelegramAuthDataDto telegramAuthDataDto)
    {
        var result = await _telegramVerifyProvider.ValidateTelegramAuthDataAsync(telegramAuthDataDto);

        return new TelegramAuthResponseDto<bool>
        {
            Success = result
        };
    }

    public async Task<TelegramAuthResponseDto<TelegramAuthDataDto>> VerifyTgBotDataAndGenerateAuthDataAsync(
        IDictionary<string, string> data)
    {
        var result = await VerifyTelegramDataAsync(data);
        if (!result)
        {
            return new TelegramAuthResponseDto<TelegramAuthDataDto>
            {
                Success = false
            };
        }

        var telegramAuthDataDto = ConvertToTelegramAuthDataDto(data);

        var hash = await _telegramVerifyProvider.GenerateHashAsync(telegramAuthDataDto);
        telegramAuthDataDto.Hash = hash;

        return new TelegramAuthResponseDto<TelegramAuthDataDto>
        {
            Success = true,
            Data = telegramAuthDataDto
        };
    }

    private async Task<bool> VerifyTelegramDataAsync(IDictionary<string, string> data)
    {
        return await _telegramVerifyProvider.ValidateTelegramDataAsync(data,
            GenerateTelegramDataHash.TgBotDataHash);
    }

    private TelegramAuthDataDto ConvertToTelegramAuthDataDto(IDictionary<string, string> data)
    {
        var userJsonString = data[CommonConstants.RequestParameterNameUser];
        var userData = JsonConvert.DeserializeObject<IDictionary<string, string>>(userJsonString);
        var telegramAuthDataDto = _objectMapper.Map<IDictionary<string, string>, TelegramAuthDataDto>(userData);
        telegramAuthDataDto.AuthDate = data.ContainsKey(CommonConstants.RequestParameterNameAuthDate)
            ? data[CommonConstants.RequestParameterNameAuthDate]
            : null;
        return telegramAuthDataDto;
    }

    public async Task<TelegramAuthResponseDto<TelegramBotInfoDto>> RegisterTelegramBot(string secret)
    {
        if (secret.IsNullOrEmpty() || !secret.Contains(Colon))
        {
            return new TelegramAuthResponseDto<TelegramBotInfoDto>()
            {
                Success = false,
                Message = "please input valid secret~"
            };
        }

        var botSecret = secret.Split(Colon);
        if (botSecret.Length != 2)
        {
            return new TelegramAuthResponseDto<TelegramBotInfoDto>()
            {
                Success = false,
                Message = "your secret format is invalid~"
            };
        }
        
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await _telegramBotRepository.AddAsync(new TelegramBotIndex()
        {
            Id = new Guid(),
            BotId = botSecret[0],
            PlaintextSecret = botSecret[1],
            Secret = _telegramVerifyProvider.EncryptSecret(botSecret[1], currentTimestamp.ToString(), botSecret[0]),
            CreateTime = currentTimestamp
        });

        var telegramBotIndex = await GetTelegramBotIndex(botSecret[0]);
        int i = 0;
        while (telegramBotIndex == null && i < 10)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            i++;
            telegramBotIndex = await GetTelegramBotIndex(botSecret[0]);
        }
        return new TelegramAuthResponseDto<TelegramBotInfoDto>()
        {
            Success = true,
            Data = new TelegramBotInfoDto()
            {
                BotId = telegramBotIndex.BotId,
                PlaintextSecret = _telegramVerifyProvider.DecryptSecret(telegramBotIndex.Secret, telegramBotIndex.CreateTime.ToString(), telegramBotIndex.BotId),
                Secret = telegramBotIndex.Secret
            }
        };
    }
    
    private async Task<TelegramBotIndex> GetTelegramBotIndex(string botId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<TelegramBotIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(f => f.BotId).Value(botId))
        };
        QueryContainer Filter(QueryContainerDescriptor<TelegramBotIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var telegramBotIndic = await _telegramBotRepository.GetListAsync(Filter);
        return telegramBotIndic.Item2.FirstOrDefault(bot => bot.BotId.Equals(botId));
    }
}