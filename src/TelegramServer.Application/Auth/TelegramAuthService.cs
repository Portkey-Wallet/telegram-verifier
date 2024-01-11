using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CATelegramServer.Auth.Dtos;
using CATelegramServer.Auth.Provider;
using CATelegramServer.Auth.Telegram;
using CATelegramServer.Common;
using CATelegramServer.Common.Dtos;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace CATelegramServer.Auth;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class TelegramAuthService : TelegramServerAppService, ITelegramAuthService
{
    private readonly ILogger<TelegramAuthService> _logger;
    private readonly ITelegramVerifyProvider _telegramVerifyProvider;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public TelegramAuthService(ILogger<TelegramAuthService> logger, ITelegramVerifyProvider telegramVerifyProvider,
        IJwtTokenProvider jwtTokenProvider)
    {
        _logger = logger;
        _telegramVerifyProvider = telegramVerifyProvider;
        _jwtTokenProvider = jwtTokenProvider;
    }

    public async Task<TelegramAuthResponseDto<JwkDto>> GetKeyAsync()
    {
        var jwkDto = await _jwtTokenProvider.GenerateJwkAsync();
        return new TelegramAuthResponseDto<JwkDto>
        {
            Success = true,
            Data = jwkDto
        };
    }

    public async Task<TelegramAuthResponseDto<bool>> VerifyTelegramAuthDataAsync(
        TelegramAuthDataDto telegramAuthDataDto)
    {
        if (!CheckInpute(telegramAuthDataDto))
        {
            return new TelegramAuthResponseDto<bool>
            {
                Success = false,
                Message = "Invalid Telegram Login Information",
            };
        }

        var result = await _telegramVerifyProvider.VerifyTelegramAuthDataAsync(telegramAuthDataDto);
        if (!result)
        {
            return new TelegramAuthResponseDto<bool>
            {
                Success = false,
                Message = "Invalid Telegram Login Information",
            };
        }

        return new TelegramAuthResponseDto<bool>
        {
            Success = true
        };
    }

    public async Task<TelegramAuthResponseDto<string>> VerifyAuthDataAndGenerateTokenAsync(
        TelegramAuthDataDto telegramAuthDataDto)
    {
        if (!CheckInpute(telegramAuthDataDto))
        {
            return new TelegramAuthResponseDto<string>
            {
                Success = false,
                Message = "Invalid Telegram Login Information",
            };
        }

        var result = await _telegramVerifyProvider.VerifyTelegramAuthDataAsync(telegramAuthDataDto);
        if (!result)
        {
            return new TelegramAuthResponseDto<string>
            {
                Success = false,
                Message = "Invalid Telegram Login Information",
            };
        }

        var token = await _jwtTokenProvider.GenerateTokenAsync(new Dictionary<string, string>()
        {
            { TelegramTokenClaimNames.UserId, telegramAuthDataDto.Id },
            { TelegramTokenClaimNames.UserName, telegramAuthDataDto.UserName },
            { TelegramTokenClaimNames.AuthDate, telegramAuthDataDto.AuthDate },
            { TelegramTokenClaimNames.FirstName, telegramAuthDataDto.FirstName },
            { TelegramTokenClaimNames.LastName, telegramAuthDataDto.LastName },
            { TelegramTokenClaimNames.Hash, telegramAuthDataDto.Hash },
            { TelegramTokenClaimNames.ProtoUrl, telegramAuthDataDto.PhotoUrl }
        });

        return new TelegramAuthResponseDto<string>
        {
            Success = true,
            Data = token
        };
    }

    private static bool CheckInpute(TelegramAuthDataDto authDataDto)
    {
        if (authDataDto == null || authDataDto.Id.IsNullOrWhiteSpace() || authDataDto.Hash.IsNullOrWhiteSpace())
        {
            return false;
        }

        return true;
    }
}