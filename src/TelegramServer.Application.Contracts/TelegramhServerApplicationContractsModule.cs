using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;

namespace CATelegramServer;

[DependsOn(
    typeof(TelegramAuthServerDomainSharedModule),
    typeof(AbpSettingManagementApplicationContractsModule)
)]
public class TelegramhServerApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        TelegramAuthServerDtoExtensions.Configure();
    }
}