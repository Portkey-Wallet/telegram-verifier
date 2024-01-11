using Volo.Abp.Modularity;

namespace TelegramServer;

[DependsOn(
    typeof(TelegramServerApplicationModule)
)]
public class CATelegramAuthServerApplicationTestModule : AbpModule
{

}
