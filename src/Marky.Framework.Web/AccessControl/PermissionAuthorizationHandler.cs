using Microsoft.AspNetCore.Authorization;

namespace Marky.Framework.Web.AccessControl;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private const string _permissionClaimType = "permission";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        var containsPermission = context.User.Claims.Any(c =>
            c.Type == _permissionClaimType
            && c.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase)
        );

        if (containsPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
