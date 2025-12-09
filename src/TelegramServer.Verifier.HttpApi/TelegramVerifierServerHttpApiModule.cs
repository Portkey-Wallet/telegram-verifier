using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace TelegramServer.Verifier;

[DependsOn(
    typeof(TelegramServerApplicationModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class TelegramVerifierServerHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        _ = context.ServiceProvider.GetService<TelegramVerifyProvider>();
    }
}