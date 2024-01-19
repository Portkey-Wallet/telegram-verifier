using Microsoft.Extensions.DependencyInjection;
using TelegramServer.Common;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace TelegramServer;

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
            
        context.Services.AddScoped<IHttpClientService, HttpClientService>();
    }
}