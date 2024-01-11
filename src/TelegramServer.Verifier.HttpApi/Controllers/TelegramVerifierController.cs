using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelegramServer.Common.Dtos;
using TelegramServer.Verifier.Options;
using Volo.Abp;

namespace TelegramServer.Verifier;

[RemoteService]
[Area("app")]
[Route("api/app/auth/")]
public class TelegramVerifierController : TelegramVerifierServerController
{
    private readonly ILogger<TelegramVerifierController> _logger;
    private readonly ITelegramVerifierService _telegramVerifierService;

    public TelegramVerifierController(ILogger<TelegramVerifierController> logger,
        ITelegramVerifierService telegramVerifierService,
        IOptions<TelegramAuthOptions> telegramAuthOptions)
    {
        _logger = logger;
        _telegramVerifierService = telegramVerifierService;
    }

    [HttpPost("verify")]
    public async Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthData(TelegramAuthDataDto authDataDto)
    {
        return await _telegramVerifierService.VerifyTelegramAuthDataAsync(authDataDto);
    }
}