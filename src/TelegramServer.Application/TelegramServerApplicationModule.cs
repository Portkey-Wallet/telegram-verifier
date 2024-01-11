using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace CATelegramServer;

[DependsOn(
    typeof(TelegramAuthServerDomainModule),
    typeof(TelegramhServerApplicationContractsModule)
)]
public class TelegramServerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<TelegramServerApplicationModule>(); });

        context.Services.AddHttpClient();
    }
}