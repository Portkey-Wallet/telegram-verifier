using TelegramServer.Localization;
using Volo.Abp.Application.Services;

namespace TelegramServer;

public class TelegramServerAuthAppService : ApplicationService
{
    protected TelegramServerAuthAppService()
    {
        LocalizationResource = typeof(TelegramAuthServerResource);
    }
}