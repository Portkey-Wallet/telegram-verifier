using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TelegramServer.Auth.Dtos;
using TelegramServer.Auth.Telegram;
using TelegramServer.Common.Dtos;
using TelegramServer.TestBase;
using Xunit;

namespace TelegramServer.Application.Tests.Auth;

[Collection(TelegramServerTestConsts.CollectionDefinitionName)]
public partial class TelegramAuthServiceTest : TelegramServerApplicationTestBase
{
    private readonly ITelegramAuthService _telegramAuthService;

    public TelegramAuthServiceTest()
    {
        _telegramAuthService = GetRequiredService<ITelegramAuthService>();
    }

    protected override void AfterAddApplication(IServiceCollection services)
    {
        services.AddSingleton(MockTelegramVerifierOptions());
        services.AddSingleton(MockHttpClientService());
        services.AddSingleton(MockJwtTokenOptions());
        services.AddSingleton(MockJwtTokenPrivateKeyProvider());
    }

    [Fact]
    public async Task GetKeyAsync_Test()
    {
        var telegramAuthResponseDto = await _telegramAuthService.GetKeyAsync();
        var a = 1;
        telegramAuthResponseDto.ShouldNotBeNull();
        var jwkDto = telegramAuthResponseDto.Data;
        jwkDto.ShouldNotBeNull();
        jwkDto.N.ShouldBe("rdnPxzGM8N6DFfXVzotWfpqFl-WN8p8_dUGOwD5p17ahKZ277EpgUEEeIQrb1-ADyQiybOg8TG_v8nljuxmioYB_ndN_t_MZOzMLomxLSGdO4kjumDB2DSxExuoL9_gcIM0qnJs9D-u9oftNayPV7k7O6DD86M10pwkf21P1jmWkiwSgACES-OJkSkyiDKAKHcaRu6FMZPWSiEn0L419SgyXJPrDjwbWer0V2kkAnVQCgLfzG3l1PWzVFLa0XZSAT2JMJdUEYpVIjXLA7ugoXFUdwxESaV_dTDt0k9Fe6VmbTLcrNocsoPJXAdP50t9V3S3Q_u4CXb0n60iKgESSSQ");
    }

    [Fact]
    public async Task VerifyTelegramAuthDataAsync_Test()
    {
        var dto = new TelegramAuthDataDto();
        var responseDto = await _telegramAuthService.VerifyTelegramAuthDataAsync(dto);
        responseDto.Success.ShouldBeFalse();
        responseDto.Message.ShouldBe("Invalid Telegram Login Information");
        
        dto = new TelegramAuthDataDto
        {
            Id = "6630865352",
            UserName = "Timm Duncann",
            AuthDate = "4093344000",
            FirstName = "Tim",
            LastName = "Duncan",
            Hash = "",
            PhotoUrl = "aa"
        };
        responseDto = await _telegramAuthService.VerifyTelegramAuthDataAsync(dto);
        responseDto.Success.ShouldBeFalse();
        responseDto.Message.ShouldBe("Invalid Telegram Login Information");

        dto.Hash = "6bd8fdba423123065b80c0d05616a789f4ae3574c648f3e6c06823fa58b526ad";
        responseDto = await _telegramAuthService.VerifyTelegramAuthDataAsync(dto);
        responseDto.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyAuthDataAndGenerateTokenAsync_Test()
    {
        var dto = new TelegramAuthDataDto();
        var responseDto = await _telegramAuthService.VerifyAuthDataAndGenerateTokenAsync(dto);
        responseDto.Success.ShouldBeFalse();
        responseDto.Message.ShouldBe("Invalid Telegram Login Information");
        dto = new TelegramAuthDataDto
        {
            Id = "6630865352",
            UserName = "Timm Duncann",
            AuthDate = "4093344000",
            FirstName = "Tim",
            LastName = "Duncan",
            Hash = "",
            PhotoUrl = "aa"
        };
        responseDto = await _telegramAuthService.VerifyAuthDataAndGenerateTokenAsync(dto);
        responseDto.Success.ShouldBeFalse();
        responseDto.Message.ShouldBe("Invalid Telegram Login Information");
        
        dto.Hash = "6bd8fdba423123065b80c0d05616a789f4ae3574c648f3e6c06823fa58b526ad";
        responseDto = await _telegramAuthService.VerifyAuthDataAndGenerateTokenAsync(dto);
        responseDto.Success.ShouldBeTrue();
        responseDto.Data.ShouldNotBeNullOrEmpty();
    }
}