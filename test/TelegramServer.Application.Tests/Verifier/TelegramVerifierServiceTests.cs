using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AElf;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TelegramServer.Common.Dtos;
using TelegramServer.TestBase;
using TelegramServer.Verifier;
using Xunit;
using Xunit.Abstractions;

namespace TelegramServer.Application.Tests.Verifier;

[Collection(TelegramServerTestConsts.CollectionDefinitionName)]
public sealed partial class TelegramVerifierServiceTests : TelegramServerApplicationTestBase
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ITelegramVerifierService _telegramVerifierService;

    public TelegramVerifierServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
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

    [Fact]
    public async Task GenerateTelegramHashTest()
    {
        Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        string initData =
            "user%3D%257B%2522id%2522%253A5990848037%252C%2522first_name%2522%253A%2522Aurora%2522%252C%2522last_name%2522%253A%2522%2522%252C%2522language_code%2522%253A%2522zh-hans%2522%252C%2522allows_write_to_pm%2522%253Atrue%257D%26chat_instance%3D-4663677144660940540%26chat_type%3Dprivate%26auth_date%3D1711428610%26hash%3D40ed9996aff8394ece63cd90212f10f81b73458f283295fcb12275ec4736d24f";
        string decodedUrl = WebUtility.UrlDecode(initData);
        decodedUrl = WebUtility.UrlDecode(decodedUrl);
        string[] parameters = decodedUrl.Split('&');
        foreach (string parameter in parameters)
        {
            string[] parts = parameter.Split('=');
            if (parts.Length == 2 && parts[0] != "hash")
            {
                string key = parts[0];
                string value = parts[1];
                keyValuePairs.Add(key, value);
            }
            else
            {
                _testOutputHelper.WriteLine($"Key: {parts[0]}, Value: (none)");
            }
        }

        // keyValuePairs.Add("user", "{\"id\":5990848037,\"first_name\":\"Aurora\",\"last_name\":\"\",\"language_code\":\"zh-hans\",\"allows_write_to_pm\":true}");
        // keyValuePairs.Add("chat_instance", "-4663677144660940540");
        // keyValuePairs.Add("chat_type", "private");
        // keyValuePairs.Add("auth_date", "1711428610");
        var sortedByKey = keyValuePairs.Keys.OrderBy(k => k);
        var sb = new StringBuilder();
        foreach (var key in sortedByKey)
        {
            sb.AppendLine($"{key}={keyValuePairs[key]}");
        }

        sb.Length = sb.Length - 1;
        string dataCheckString = sb.ToString();

        var webAppDataBytes = Encoding.UTF8.GetBytes("WebAppData");
        using (var webAppDataHmacsha256 = new HMACSHA256(webAppDataBytes))
        {
            var tokenBytes =
                webAppDataHmacsha256.ComputeHash(
                    Encoding.UTF8.GetBytes("6741218435:AAEMuSu3u0y4FXVqGPuW1F5RoQ5kudW_0Xs"));
            using (var tokenHmacsha256 = new HMACSHA256(tokenBytes))
            {
                var hashBytes = tokenHmacsha256.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
                var hash = hashBytes.ToHex();
                _testOutputHelper.WriteLine(
                    ("**:" + ("40ed9996aff8394ece63cd90212f10f81b73458f283295fcb12275ec4736d24f") == hash).ToString());
            }
        }
    }

    [Fact]
    public async Task GenerateTelegramHashForAppTest()
    {
        TelegramAuthDataDto telegramAuthDataDto = new TelegramAuthDataDto
        {
            Id = "5990848037",
            UserName = null,
            AuthDate = "1712528610",
            FirstName = "Aurora",
            LastName = null,
            Hash = "a968a40b2f412a317ed13b0814e682ce03498e781e9719e1b674be88ebc1cb0f",
            PhotoUrl = null
        };

        var dataCheckString = GetDataCheckString(telegramAuthDataDto);
        var hash = await GenerateTelegramHashAsync("6741218435:AAEMuSu3u0y4FXVqGPuW1F5RoQ5kudW_0Xs", dataCheckString);
        _testOutputHelper.WriteLine(("**:" + hash));
    }

    private static string GetDataCheckString(TelegramAuthDataDto telegramAuthDataDto)
    {
        Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        if (!telegramAuthDataDto.Id.IsNullOrWhiteSpace())
        {
            keyValuePairs.Add("id", telegramAuthDataDto.Id);
        }

        if (telegramAuthDataDto.UserName != null)
        {
            keyValuePairs.Add("username", telegramAuthDataDto.UserName);
        }

        if (telegramAuthDataDto.AuthDate != null)
        {
            keyValuePairs.Add("auth_date", telegramAuthDataDto.AuthDate);
        }

        if (telegramAuthDataDto.FirstName != null)
        {
            keyValuePairs.Add("first_name", telegramAuthDataDto.FirstName);
        }

        if (telegramAuthDataDto.LastName != null)
        {
            keyValuePairs.Add("last_name", telegramAuthDataDto.LastName);
        }

        if (telegramAuthDataDto.PhotoUrl != null)
        {
            keyValuePairs.Add("photo_url", telegramAuthDataDto.PhotoUrl);
        }

        var sortedByKey = keyValuePairs.Keys.OrderBy(k => k);
        var sb = new StringBuilder();
        foreach (var key in sortedByKey)
        {
            sb.AppendLine($"{key}={keyValuePairs[key]}");
        }

        sb.Length = sb.Length - 1;
        return sb.ToString();
    }

    private static Task<string> GenerateTelegramHashAsync(string token, string dataCheckString)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var dataCheckStringBytes = Encoding.UTF8.GetBytes(dataCheckString);
        var computeFrom = HashHelper.ComputeFrom(tokenBytes).ToByteArray();

        using var hmac = new HMACSHA256(computeFrom);
        var hashBytes = hmac.ComputeHash(dataCheckStringBytes);
        return Task.FromResult(hashBytes.ToHex());
    }
}