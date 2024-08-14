using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TelegramServer.Common.Dtos;
using TelegramServer.Verifier.Dto;
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

    [HttpPost("bot/verify")]
    public async Task<TelegramAuthResponseDto<TelegramAuthDataDto>> VerifyTelegramBotAuthData()
    {
        var streamReader = new StreamReader(Request.Body);
        var requestJson = await streamReader.ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<IDictionary<string, string>>(requestJson);
        return await _telegramVerifierService.VerifyTgBotDataAndGenerateAuthDataAsync(data);
    }
    
    [HttpPost("bot/register")]
    public async Task<TelegramAuthResponseDto<TelegramBotDto>> RegisterTelegramBot([FromHeader] string applicant, [FromBody] RegisterTelegramBotDto registerTelegramBotDto)
    {
        _logger.LogInformation(".....RegisterTelegramBot Applicant is:{0}", applicant);
        return await _telegramVerifierService.RegisterTelegramBot(registerTelegramBotDto.Secret);
    }
}