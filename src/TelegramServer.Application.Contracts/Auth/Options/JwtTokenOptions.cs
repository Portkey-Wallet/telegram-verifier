namespace TelegramServer.Auth.Options;

public class JwtTokenOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int Expire { get; set; }
}