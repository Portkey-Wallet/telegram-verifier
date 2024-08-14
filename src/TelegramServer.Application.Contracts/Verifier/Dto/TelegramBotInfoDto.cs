namespace TelegramServer.Verifier.Dto;

public class TelegramBotInfoDto
{
    public string BotId { get; set; }
    public string PlaintextSecret { get; set; }
    public string Secret { get; set; }
}