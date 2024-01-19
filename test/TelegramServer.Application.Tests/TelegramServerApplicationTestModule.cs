using Microsoft.Extensions.DependencyInjection;
using TelegramServer.TestBase;
using TelegramServer.Verifier;
using Volo.Abp.Modularity;

namespace TelegramServer.Application.Tests;

[DependsOn(
    typeof(TelegramServerApplicationModule),
    typeof(TelegramServerTestBaseModule)

)]
public class TelegramServerApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ITelegramVerifyProvider, TelegramVerifyProvider>();
        context.Services.AddSingleton<TelegramVerifierService>();
        base.ConfigureServices(context);
    }
}
