using System.Threading.Tasks;
using CATelegramServer.Auth.Dtos;
using CATelegramServer.Auth.Telegram;
using CATelegramServer.Common.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace CATelegramServer.Auth;

[RemoteService]
[Area("app")]
[ControllerName("TelegramAuth")]
[Route("api/app/auth/")]
public class TelegramAuthController : TelegramAuthServerController
{
    private readonly ILogger<TelegramAuthController> _logger;
    private readonly ITelegramAuthService _telegramAuthService;

    public TelegramAuthController(ILogger<TelegramAuthController> logger, ITelegramAuthService telegramAuthService)
    {
        _logger = logger;
        _telegramAuthService = telegramAuthService;
    }

    [HttpGet]
    [Route("key")]
    public async Task<TelegramAuthResponseDto<JwkDto>> GetKey()
    {
        return await _telegramAuthService.GetKeyAsync();
    }

    [HttpPost("verify")]
    public async Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthData(TelegramAuthDataDto authDataDto)
    {
        return await _telegramAuthService.VerifyTelegramAuthDataAsync(authDataDto);
    }

    [HttpPost("token")]
    public async Task<TelegramAuthResponseDto<string>> VerifyAuthDataAndGenerateToken(TelegramAuthDataDto authDataDto)
    {
        return await _telegramAuthService.VerifyAuthDataAndGenerateTokenAsync(authDataDto);
    }
}