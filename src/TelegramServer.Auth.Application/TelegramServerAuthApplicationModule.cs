using Microsoft.Extensions.DependencyInjection;
using TelegramServer.Common;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace TelegramServer;

[DependsOn(
    typeof(TelegramAuthServerAuthDomainModule),
    typeof(TelegramServerAuthApplicationContractsModule),
    typeof(AbpAutoMapperModule)
)]
public class TelegramServerAuthApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<TelegramServerAuthApplicationModule>(); });

        context.Services.AddHttpClient();

        context.Services.AddScoped<IHttpClientService, HttpClientService>();
    }
}