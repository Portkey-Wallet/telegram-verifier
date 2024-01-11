using CATelegramServer.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace CATelegramServer.Verifier;

/* Inherit your controllers from this class.
 */
public abstract class TelegramVerifierServerController : AbpControllerBase
{
    protected TelegramVerifierServerController()
    {
        LocalizationResource = typeof(TelegramAuthServerResource);
    }
}