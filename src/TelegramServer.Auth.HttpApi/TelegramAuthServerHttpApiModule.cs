using Microsoft.Extensions.DependencyInjection;
using TelegramServer.Auth.Provider;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace TelegramServer.Auth;

[DependsOn(
    typeof(TelegramServerApplicationModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class TelegramAuthServerHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<TelegramAuthServerHttpApiModule>(); });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        _ = context.ServiceProvider.GetService<JwtTokenProvider>();
    }
}