using System.ComponentModel.DataAnnotations;

namespace TelegramServer.Verifier.Dto;

public class RegisterTelegramBotDto
{
    [Required]
    public string Secret { get; set; }
}