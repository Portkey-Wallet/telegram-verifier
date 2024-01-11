using System.Threading.Tasks;
using CATelegramServer.Auth.Dtos;
using CATelegramServer.Common.Dtos;

namespace CATelegramServer.Auth.Telegram;

public interface ITelegramAuthService
{
    Task<TelegramAuthResponseDto<JwkDto>> GetKeyAsync();
    Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);
    Task<TelegramAuthResponseDto<string>> VerifyAuthDataAndGenerateTokenAsync(TelegramAuthDataDto telegramAuthDataDto);
}