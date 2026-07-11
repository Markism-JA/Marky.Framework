using Microsoft.AspNetCore.Authorization;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Marky.Framework.Web;

public class SecurityRequirementsOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var hasAllowAnonymous =
            context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
            || context
                .MethodInfo.DeclaringType!.GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any();

        if (hasAllowAnonymous)
        {
            return true;
        }

        var hasAuthorize =
            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || context
                .MethodInfo.DeclaringType!.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any();

        if (hasAuthorize)
        {
            context.OperationDescription.Operation.Security ??=
                new List<NSwag.OpenApiSecurityRequirement>();
            context.OperationDescription.Operation.Security.Add(
                new NSwag.OpenApiSecurityRequirement { { "JWT", Array.Empty<string>() } }
            );
        }

        return true;
    }
}
