namespace TelegramServer.Auth.Dtos;

public class JwkDto
{
    public string Kty { get; set; } = "RSA";
    public string Alg { get; set; } = "RS256";
    public string Use { get; set; } = "sig";
    public string Kid { get; set; } = "Tg-Key";
    public string N { get; set; }
    public string E { get; set; }
}