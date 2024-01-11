using System.Threading.Tasks;
using TelegramServer.Common.Dtos;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace TelegramServer.Verifier;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class TelegramVerifierService : TelegramServerAppService, ITelegramVerifierService
{
    private readonly ITelegramVerifyProvider _telegramVerifyProvider;

    public TelegramVerifierService(ITelegramVerifyProvider telegramVerifyProvider)
    {
        _telegramVerifyProvider = telegramVerifyProvider;
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
}