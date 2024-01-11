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
using TelegramServer.Common;
using Volo.Abp.DependencyInjection;

namespace TelegramServer.Auth.Provider;

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

    public JwtTokenProvider(ILogger<JwtTokenProvider> logger, IOptionsSnapshot<JwtTokenOptions> jwtTokenOptions)
    {
        _logger = logger;
        _jwtTokenOptions = jwtTokenOptions.Value;
        LoadPrivateKey();
    }

    private void LoadPrivateKey()
    {
        _logger.LogInformation("Wait for the input of the key....");
        Task.Delay(1000);
        Console.WriteLine();

        Console.WriteLine("Enter the key used to generate the Jwt Token and press enter");
        while (!InputAndCheckKey())
        {
        }

        Console.WriteLine("\nFinished....");
        Console.WriteLine();
    }

    private bool InputAndCheckKey()
    {
        try
        {
            Console.Write("The key is: ");
            var key = ConsoleHelper.ReadKey();
            key = key.Replace("\\n", "\n");
            var privateKey = Convert.FromBase64String(key);
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportPkcs8PrivateKey(privateKey, out _);
            var rsaParameters = rsa.ExportParameters(false);
            var n = Base64UrlEncoder.Encode(rsaParameters.Modulus);
            var e = Base64UrlEncoder.Encode(rsaParameters.Exponent);
            _key = key;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed!");
            return false;
        }
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