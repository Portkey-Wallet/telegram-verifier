using System;
using Microsoft.Extensions.Options;
using Moq;
using TelegramServer.Common.Dtos;
using TelegramServer.Verifier;
using TelegramServer.Verifier.Options;

namespace TelegramServer.Application.Tests.Verifier;

public partial class TelegramVerifierServiceTests
{
    private static IOptions<TelegramAuthOptions> MockTelegramAuthOptions()
    {
        var mockOptions = new Mock<IOptions<TelegramAuthOptions>>();

        mockOptions.Setup(o => o.Value).Returns(
            new TelegramAuthOptions
            {
                Expire = 3600
            });
        return mockOptions.Object;
    }

    private static ITelegramTokenProvider MockTelegramTokenProvider()
    {
        var mock = new Mock<ITelegramTokenProvider>();
        mock.Setup(o => o.LoadToken()).Returns("6741218435:AAEMuSu3u0y4FXVqGPuW1F5RoQ5kudW_0Xs");
        return mock.Object;
    }

    private static ITelegramVerifyProvider MockTelegramVerifyProvider()
    {
        var mock = new Mock<ITelegramVerifyProvider>();
        mock.Setup(o => o.ValidateTelegramAuthDataAsync(It.IsAny<TelegramAuthDataDto>())).ReturnsAsync(
            (TelegramAuthDataDto dot) =>
            {
                if (dot.Hash.IsNullOrWhiteSpace())
                {
                    return false;
                }

                return true;
            });
            
        return mock.Object;
    }
}