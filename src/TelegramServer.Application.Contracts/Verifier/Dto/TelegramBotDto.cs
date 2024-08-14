namespace TelegramServer.Verifier.Dto;

public class TelegramBotDto
{
    public string BotId { get; set; }
    public string PlaintextSecret { get; set; }
    public string Secret { get; set; }
}