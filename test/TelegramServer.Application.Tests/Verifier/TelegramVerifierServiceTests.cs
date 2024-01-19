using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TelegramServer.Common.Dtos;
using TelegramServer.TestBase;
using TelegramServer.Verifier;
using Xunit;

namespace TelegramServer.Application.Tests.Verifier;

[Collection(TelegramServerTestConsts.CollectionDefinitionName)]
public sealed partial class TelegramVerifierServiceTests : TelegramServerApplicationTestBase
{
    private readonly ITelegramVerifierService _telegramVerifierService;

    public TelegramVerifierServiceTests()
    {
        _telegramVerifierService = GetRequiredService<ITelegramVerifierService>();
    }

    protected override void BeforeAddApplication(IServiceCollection services)
    {
    }

    protected override void AfterAddApplication(IServiceCollection services)
    {
        services.AddSingleton(MockTelegramAuthOptions());
        services.AddSingleton(MockTelegramTokenProvider());
        // services.AddSingleton(MockTelegramVerifyProvider());
    }

    [Fact]
    public async Task VerifyTelegramAuthDataAsync_Test()
    {
        var telegramAuthDataDto = new TelegramAuthDataDto
        {
            Id = null,
            UserName = null,
            AuthDate = null,
            FirstName = null,
            LastName = null,
            Hash = null,
            PhotoUrl = null
        };
        TelegramAuthResponseDto<bool> result =
            await _telegramVerifierService.VerifyTelegramAuthDataAsync(telegramAuthDataDto);
        result.Success.ShouldBe(false);

        telegramAuthDataDto.Id = "6630865352";
        telegramAuthDataDto.UserName = "Timm Duncann";
        telegramAuthDataDto.AuthDate = "4093344000";
        telegramAuthDataDto.FirstName = "Tim";
        telegramAuthDataDto.LastName = "Duncan";
        telegramAuthDataDto.Hash = "0e8d4e570d986fdffb1b161da1ac4fa5364a12b8b6de488739617425a4cf3124";
        telegramAuthDataDto.PhotoUrl = "aa";
        result =
            await _telegramVerifierService.VerifyTelegramAuthDataAsync(telegramAuthDataDto);
        result.Success.ShouldBe(false);

        telegramAuthDataDto.Hash = "6bd8fdba423123065b80c0d05616a789f4ae3574c648f3e6c06823fa58b526ad";
        result =
            await _telegramVerifierService.VerifyTelegramAuthDataAsync(telegramAuthDataDto);
        result.Success.ShouldBe(true);
    }
}