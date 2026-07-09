using Microsoft.AspNetCore.Authorization;

namespace Marky.Framework.Web.AccessControl;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
