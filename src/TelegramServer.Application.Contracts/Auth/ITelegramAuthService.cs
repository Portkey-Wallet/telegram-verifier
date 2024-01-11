using System.Threading.Tasks;
using TelegramServer.Auth.Dtos;
using TelegramServer.Common.Dtos;

namespace TelegramServer.Auth.Telegram;

public interface ITelegramAuthService
{
    Task<TelegramAuthResponseDto<JwkDto>> GetKeyAsync();
    Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);
    Task<TelegramAuthResponseDto<string>> VerifyAuthDataAndGenerateTokenAsync(TelegramAuthDataDto telegramAuthDataDto);
}