using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TelegramServer.Auth.Dtos;
using TelegramServer.Common;
using TelegramServer.Common.Dtos;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace TelegramServer.Auth;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class TelegramAuthService : TelegramServerAuthAppService, ITelegramAuthService
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
            { TelegramTokenClaimNames.ProtoUrl, telegramAuthDataDto.PhotoUrl },
            { TelegramTokenClaimNames.BotId, telegramAuthDataDto.BotId }
        });

        return new TelegramAuthResponseDto<string>
        {
            Success = true,
            Data = token
        };
    }

    public async Task<TelegramAuthResponseDto<string>> VerifyTgBotAuthDataAndGenerateTokenAsync(
        [CanBeNull] IDictionary<string, string> data)
    {
        if (data.IsNullOrEmpty())
        {
            _logger.LogError("dataCheck is null");
            return new TelegramAuthResponseDto<string>
            {
                Success = false,
                Message = "Invalid Telegram Login Information",
            };
        }

        var result = await _telegramVerifyProvider.VerifyTelegramBotAuthDataAsync(data);
        if (result == null)
        {
            _logger.LogError("Verify telegram bot auth data return null");
            return new TelegramAuthResponseDto<string>
            {
                Success = false,
                Message = "Invalid Telegram Login Information",
            };
        }

        var token = await _jwtTokenProvider.GenerateTokenAsync(new Dictionary<string, string>()
        {
            { TelegramTokenClaimNames.UserId, result.Id },
            { TelegramTokenClaimNames.UserName, result.UserName },
            { TelegramTokenClaimNames.AuthDate, result.AuthDate },
            { TelegramTokenClaimNames.FirstName, result.FirstName },
            { TelegramTokenClaimNames.LastName, result.LastName },
            { TelegramTokenClaimNames.Hash, result.Hash },
            { TelegramTokenClaimNames.ProtoUrl, result.PhotoUrl },
            { TelegramTokenClaimNames.BotId, result.BotId }
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