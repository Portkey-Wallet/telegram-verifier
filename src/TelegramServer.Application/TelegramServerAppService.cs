using CATelegramServer.Localization;
using Volo.Abp.Application.Services;

namespace CATelegramServer;

/* Inherit your application services from this class.
 */
public abstract class TelegramServerAppService : ApplicationService
{
    protected TelegramServerAppService()
    {
        LocalizationResource = typeof(TelegramAuthServerResource);
    }
}