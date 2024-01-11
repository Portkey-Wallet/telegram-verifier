using System.Threading.Tasks;
using CATelegramServer.Common.Dtos;

namespace CATelegramServer.Verifier;

public interface ITelegramVerifierService
{
    Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);
}