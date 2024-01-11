using CATelegramServer.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace CATelegramServer.Permissions;

public class TelegramAuthServerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TelegramAuthServerPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(CATelegramAuthServerPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TelegramAuthServerResource>(name);
    }
}