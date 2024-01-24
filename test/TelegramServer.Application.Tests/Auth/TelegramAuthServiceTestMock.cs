using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using TelegramServer.Auth;
using TelegramServer.Auth.Options;
using TelegramServer.Common;

namespace TelegramServer.Application.Tests.Auth;

public partial class TelegramAuthServiceTest
{
    private IOptions<TelegramVerifierOptions> MockTelegramVerifierOptions()
    {
        var mock = new Mock<IOptions<TelegramVerifierOptions>>();
        mock.Setup(o => o.Value).Returns(new TelegramVerifierOptions
        {
            Url = "127.0.0.1:8080",
            Timeout = 10
        });
        return mock.Object;
    }

    private IOptions<JwtTokenOptions> MockJwtTokenOptions()
    {
        var mock = new Mock<IOptions<JwtTokenOptions>>();
        mock.Setup(o => o.Value).Returns(new JwtTokenOptions
        {
            Issuer = "PortKey",
            Audience = "PortKey",
            Expire = 36000
        });
        return mock.Object;
    }

    private IHttpClientService MockHttpClientService()
    {
        var mock = new Mock<IHttpClientService>();
        mock.Setup(o => o.PostAsync<TelegramAuthResponseDto<bool>>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync((string url, object param) =>
            {
                if (((Dictionary<string, string>)param)["Hash"].IsNullOrWhiteSpace())
                {
                    return new TelegramAuthResponseDto<bool>
                    {
                        Success = false,
                        Message = null,
                        Data = false
                    };
                }

                return new TelegramAuthResponseDto<bool>
                {
                    Success = true,
                    Message = null,
                    Data = false
                };
            });
        return mock.Object;
    }

    private IJwtTokenPrivateKeyProvider MockJwtTokenPrivateKeyProvider()
    {
        var mock = new Mock<IJwtTokenPrivateKeyProvider>();
        mock.Setup(o => o.LoadPrivateKey()).Returns(
            "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCt2c/HMYzw3oMV\n9dXOi1Z+moWX5Y3ynz91QY7APmnXtqEpnbvsSmBQQR4hCtvX4APJCLJs6DxMb+/y\neWO7GaKhgH+d03+38xk7MwuibEtIZ07iSO6YMHYNLETG6gv3+BwgzSqcmz0P672h\n+01rI9XuTs7oMPzozXSnCR/bU/WOZaSLBKAAIRL44mRKTKIMoAodxpG7oUxk9ZKI\nSfQvjX1KDJck+sOPBtZ6vRXaSQCdVAKAt/MbeXU9bNUUtrRdlIBPYkwl1QRilUiN\ncsDu6ChcVR3DERJpX91MO3ST0V7pWZtMtys2hyyg8lcB0/nS31XdLdD+7gJdvSfr\nSIqARJJJAgMBAAECggEAFzS303L9yTkqbkf+Xi+jQAsIQhpWNEilg9VYZLjwD+dE\n4/U2HHhhGtVWXDrC1bIFT1Rl3LuzMNDQSqGBenwVhVFNt5d/uOIuQAS0TN3/vo5r\nrTDnCFGPsHp0Q8kCB/uKZaZ9RaJDKFjjWzfcQazq5YCHhd7kI38pTxKcuf4fDP6z\n59ulLdzGOaRkuYwhHCJlKBN0MQ4ldNU3dcGak5nGK2VV6oT2bjq5ek5Go2agtZKG\ny4hjAO+fcJpshp2wZOvx4A8mW2ae1YAwKgZUaaCa1C2qfQkrWjkLi+lltbL14yUq\neEO5dKm4GE2DjUjmbhSUy1laJ+WS/aeEoRPRySTJsQKBgQDgDGzgh6q/hkU3nek4\ndmIbDZobqY5SV/GwISt5CnZ1uD44KvOikn/kWWY2wzJlcVekl3+sNtgZo3E1Mc+3\n8T8RLMYS3S5rHaU0Tt3t+AQ0HvpqOGuS+Rd7gosScDXJ5pq6nI0pTXjA56BinOJX\n24XRkn3YtSFWqlL4Zzy/ehGrKwKBgQDGpMQU4Heca2We+spX52AxenEqNC+U8eWs\n8Eba+lrAt4Mu3wVYYJNcvMvINx/ZrGr5LuRGJux/EAmFZsJCdoHzSmEKRRxKfM1D\n0jtwhasNRD7/jGmN2qyliO4GKPkBG7TDf9+CivfOk5TMKZVmxBu+llv/RmmsqOxm\nv501hDEuWwKBgEML9NkMQFCoQbZvDwfShXFLFL8Kcoi5wJ3Qj03dj62SwSvVzqrr\n5FHVXv0sVLx+upeKrq4+i1TA0HP2wA5vp0vgdjXW8rkjWfjZURiRi9B9JLr8v1Rw\nLlLLsgqGgdI1rEAD8UpW4Lf5mMlp4WIhU15v1DExoxRoTAQCAO/b+8WZAoGAfFaa\nHe87L3/SBic5DLjZb18TArXTqsmXWB62W08mC0dTJ72VnPImi3/plpNarmfMNdly\nLa8jjY0+SHA/3FZNlTnTcBg+Uym3WmJ3rkEdBprXTCJZ198u/hat1tFCu7zZ8x1R\njGbsIjQiiYDl8YODUlLPlwQ+FUNPZik+gEcUec0CgYAT4wa9O13etSu31dGXVzbl\nHRSpTvtf+CixwoqQf4yG29/MO5fGJzWTgqKjPf0DSoJ4JxUYUcJwco9Kp18MPmdH\n0mxLVIilHMHmK9L81qag+VKiQOxa1N0T3FG2fwrBrYX1y7mEUq+SRQo61Rwx451m\nvIvyCjEahVlx8/wy9NL2tA==");
        return mock.Object;
    }
}