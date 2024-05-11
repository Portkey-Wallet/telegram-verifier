using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramServer.Common.Dtos;

namespace TelegramServer.Verifier;

public interface ITelegramVerifierService
{
    Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);

    Task<TelegramAuthResponseDto<TelegramAuthDataDto>> VerifyTgBotDataAndGenerateAuthDataAsync(
        IDictionary<string, string> data);
}