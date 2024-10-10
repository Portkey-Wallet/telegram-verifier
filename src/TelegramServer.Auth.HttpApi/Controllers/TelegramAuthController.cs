using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TelegramServer.Auth.Dtos;
using TelegramServer.Auth.Telegram;
using TelegramServer.Common.Dtos;
using Volo.Abp;

namespace TelegramServer.Auth;

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

    [HttpPost("bot/token")]
    public async Task<TelegramAuthResponseDto<string>> VerifyTgBotAuthDataAndGenerateToken()
    {
        var streamReader = new StreamReader(Request.Body);
        var requestJson = await streamReader.ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<IDictionary<string, string>>(requestJson);
        return await _telegramAuthService.VerifyTgBotAuthDataAndGenerateTokenAsync(data);
    }
}