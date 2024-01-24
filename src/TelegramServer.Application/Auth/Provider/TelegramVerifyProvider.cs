using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelegramServer.Auth.Options;
using TelegramServer.Common;
using TelegramServer.Common.Dtos;
using Volo.Abp.DependencyInjection;

namespace TelegramServer.Auth;

public interface ITelegramVerifyProvider
{
    Task<bool> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto);
}

public class TelegramVerifyProvider : ITelegramVerifyProvider, ISingletonDependency
{
    private readonly ILogger<TelegramVerifyProvider> _logger;
    private readonly IHttpClientService _httpClientService;
    private readonly TelegramVerifierOptions _telegramVerifierOptions;
    private readonly string _url;

    private const string VerifyUrl = "api/app/auth/verify";

    public TelegramVerifyProvider(
        ILogger<TelegramVerifyProvider> logger,
        IOptions<TelegramVerifierOptions> telegramVerifierOptions,
        IHttpClientService httpClientService)
    {
        _logger = logger;
        _httpClientService = httpClientService;
        _telegramVerifierOptions = telegramVerifierOptions.Value;
        _url = GetUrl(VerifyUrl);
    }

    public async Task<bool> VerifyTelegramAuthDataAsync(TelegramAuthDataDto telegramAuthDataDto)
    {
        var properties = telegramAuthDataDto.GetType().GetProperties();
        var param = properties.ToDictionary(property => property.Name,
            property => property.GetValue(telegramAuthDataDto)?.ToString());
        var telegramAuthResponseDto = await _httpClientService.PostAsync<TelegramAuthResponseDto<bool>>(_url, param);

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