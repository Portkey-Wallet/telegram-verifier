using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TelegramServer.Common;
using TelegramServer.Common.Dtos;
using TelegramServer.Verifier.Provider;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace TelegramServer.Verifier;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class TelegramVerifierService : TelegramServerAppService, ITelegramVerifierService
{
    private readonly ITelegramVerifyProvider _telegramVerifyProvider;
    private readonly IObjectMapper _objectMapper;

    public TelegramVerifierService(ITelegramVerifyProvider telegramVerifyProvider, IObjectMapper objectMapper)
    {
        _telegramVerifyProvider = telegramVerifyProvider;
        _objectMapper = objectMapper;
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
}