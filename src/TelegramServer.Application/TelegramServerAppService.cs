using TelegramServer.Localization;
using Volo.Abp.Application.Services;

namespace TelegramServer;

/* Inherit your application services from this class.
 */
public abstract class TelegramServerAppService : ApplicationService
{
    protected TelegramServerAppService()
    {
        LocalizationResource = typeof(TelegramAuthServerResource);
    }
}