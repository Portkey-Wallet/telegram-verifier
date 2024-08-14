using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramServer.Common.Dtos;
using TelegramServer.Verifier.Dto;

namespace TelegramServer.Verifier;

public interface ITelegramVerifierService
{
    Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);

    Task<TelegramAuthResponseDto<TelegramAuthDataDto>> VerifyTgBotDataAndGenerateAuthDataAsync(
        IDictionary<string, string> data);

    Task<TelegramAuthResponseDto<TelegramBotInfoDto>> RegisterTelegramBot(string secret);
}