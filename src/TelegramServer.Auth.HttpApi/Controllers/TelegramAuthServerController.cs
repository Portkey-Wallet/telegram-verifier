using CATelegramServer.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace CATelegramServer.Auth;

/* Inherit your controllers from this class.
 */
public abstract class TelegramAuthServerController : AbpControllerBase
{
    protected TelegramAuthServerController()
    {
        LocalizationResource = typeof(TelegramAuthServerResource);
    }
}