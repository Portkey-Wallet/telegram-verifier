using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TelegramServer.Auth.Dtos;
using TelegramServer.Auth.Options;
using Volo.Abp.DependencyInjection;

namespace TelegramServer.Auth;

public interface IJwtTokenProvider
{
    Task<JwkDto> GenerateJwkAsync();
    Task<string> GenerateTokenAsync(IDictionary<string, string> userInfo);
}

public class JwtTokenProvider : IJwtTokenProvider, ISingletonDependency
{
    private readonly ILogger<JwtTokenProvider> _logger;
    private readonly JwtTokenOptions _jwtTokenOptions;
    private string _key;

    public JwtTokenProvider(ILogger<JwtTokenProvider> logger, IOptions<JwtTokenOptions> jwtTokenOptions,
        IJwtTokenPrivateKeyProvider jwtTokenPrivateKeyProvider)
    {
        _logger = logger;
        _jwtTokenOptions = jwtTokenOptions.Value;
        _key = jwtTokenPrivateKeyProvider.LoadPrivateKey();
    }

    public Task<JwkDto> GenerateJwkAsync()
    {
        var privateKey = Convert.FromBase64String(_key);
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportPkcs8PrivateKey(privateKey, out _);
        var rsaParameters = rsa.ExportParameters(false);

        return Task.FromResult(new JwkDto
        {
            N = Base64UrlEncoder.Encode(rsaParameters.Modulus),
            E = Base64UrlEncoder.Encode(rsaParameters.Exponent)
        });
    }

    public async Task<string> GenerateTokenAsync(IDictionary<string, string> userInfo)
    {
        var privateKey = Convert.FromBase64String(_key);
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportPkcs8PrivateKey(privateKey, out _);
            AsymmetricSecurityKey asymmetricSecurityKey = new RsaSecurityKey(rsa.ExportParameters(true));
            var signingCredentials = new SigningCredentials(asymmetricSecurityKey, SecurityAlgorithms.RsaSha256);

            var claims = new List<Claim>
            {
                Capacity = userInfo.Count
            };
            foreach (var keyValuePair in userInfo)
            {
                if (keyValuePair.Value != null)
                {
                    claims.Add(new(keyValuePair.Key, keyValuePair.Value));
                }
            }

            var token = new JwtSecurityToken(
                issuer: _jwtTokenOptions.Issuer,
                audience: _jwtTokenOptions.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddSeconds(_jwtTokenOptions.Expire),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}