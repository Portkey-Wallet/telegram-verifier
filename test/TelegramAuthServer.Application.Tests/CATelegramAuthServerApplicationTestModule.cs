using Volo.Abp.Modularity;

namespace CATelegramServer;

[DependsOn(
    typeof(TelegramServerApplicationModule)
)]
public class CATelegramAuthServerApplicationTestModule : AbpModule
{

}
