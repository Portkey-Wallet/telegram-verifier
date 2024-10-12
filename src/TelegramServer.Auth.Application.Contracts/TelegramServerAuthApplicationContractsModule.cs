using TelegramServer.Auth.Application.Contracts;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;

namespace TelegramServer;

[DependsOn(
    typeof(TelegramAuthServerAuthDomainSharedModule),
    typeof(AbpSettingManagementApplicationContractsModule)
)]
public class TelegramServerAuthApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        TelegramAuthServerDtoExtensions.Configure();
    }
}