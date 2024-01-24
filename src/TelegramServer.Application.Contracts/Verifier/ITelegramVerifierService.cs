using System.Threading.Tasks;
using TelegramServer.Common.Dtos;

namespace TelegramServer.Verifier;

public interface ITelegramVerifierService
{
    Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);
}