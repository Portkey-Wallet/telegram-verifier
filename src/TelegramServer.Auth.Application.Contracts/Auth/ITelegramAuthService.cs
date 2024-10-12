using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramServer.Auth.Dtos;
using TelegramServer.Common.Dtos;

namespace TelegramServer.Auth;

public interface ITelegramAuthService
{
    Task<TelegramAuthResponseDto<JwkDto>> GetKeyAsync();
    Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);
    Task<TelegramAuthResponseDto<string>> VerifyAuthDataAndGenerateTokenAsync(TelegramAuthDataDto telegramAuthDataDto);
    Task<TelegramAuthResponseDto<string>> VerifyTgBotAuthDataAndGenerateTokenAsync(IDictionary<string, string> data);
}