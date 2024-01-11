using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CATelegramServer.Auth.Options;
using CATelegramServer.Common;
using CATelegramServer.Common.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace CATelegramServer.Auth;

public interface ITelegramVerifyProvider
{
    Task<bool> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);
}

public class TelegramVerifyProvider : ITelegramVerifyProvider, ISingletonDependency
{
    private readonly ILogger<TelegramVerifyProvider> _logger;
    private readonly IHttpService _httpService;
    private readonly TelegramVerifierOptions _telegramVerifierOptions;
    private readonly string _url;

    private const string VerifyUrl = "api/app/auth/verify";

    public TelegramVerifyProvider(
        ILogger<TelegramVerifyProvider> logger,
        IOptions<TelegramVerifierOptions> telegramVerifierOptions,
        IHttpClientFactory httpClientFactory
    )
    {
        _logger = logger;
        _telegramVerifierOptions = telegramVerifierOptions.Value;
        _httpService = new HttpService(telegramVerifierOptions.Value.Timeout, httpClientFactory, true);
        _url = GetUrl(VerifyUrl);
    }

    public async Task<bool> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto)
    {
        var properties = telegramAuthDataDto.GetType().GetProperties();
        var param = properties.ToDictionary(property => property.Name,
            property => property.GetValue(telegramAuthDataDto)?.ToString());
        var telegramAuthResponseDto = await _httpService.PostResponseAsync<TelegramAuthResponseDto<bool>>(_url, param);

        if (telegramAuthResponseDto == null)
        {
            _logger.LogError("verifier interface return null");
            return false;
        }

        if (!telegramAuthResponseDto.Success)
        {
            _logger.LogError("Validation failed: {0}", telegramAuthResponseDto.Message);
        }

        return telegramAuthResponseDto.Success;
    }

    private string GetUrl(string url)
    {
        return $"{_telegramVerifierOptions.Url.TrimEnd('/')}/{url}";
    }
}