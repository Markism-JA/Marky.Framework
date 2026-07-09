using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Marky.Framework.Web;

public class AutoErrorResponseProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var operation = context.OperationDescription.Operation;

        AddResponseIfMissing(operation, "400", "Validation or model state failure.", context);
        AddResponseIfMissing(operation, "500", "Internal system kernel execution crash.", context);

        var authorizeAttributes = context
            .MethodInfo.GetCustomAttributes(inherit: true)
            .Concat(
                context.MethodInfo.DeclaringType?.GetCustomAttributes(inherit: true)
                    ?? Array.Empty<object>()
            )
            .OfType<AuthorizeAttribute>()
            .ToList();

        if (authorizeAttributes.Count != 0)
        {
            AddResponseIfMissing(
                operation,
                "401",
                "Unauthorized - Missing, invalid, or expired Bearer JWT Tokens",
                context
            );

            var requiredPermissions = authorizeAttributes
                .Select(a => a.Policy)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            string forbiddenDescription =
                requiredPermissions.Count != 0
                    ? $"Forbidden - Account lacks one of the required permission claims: [{string.Join(", ", requiredPermissions)}]."
                    : "Forbidden - Account has been deactivated or lacks necessary permissions.";
            AddResponseIfMissing(operation, "403", forbiddenDescription, context);
        }
        return true;
    }

    private static void AddResponseIfMissing(
        OpenApiOperation operation,
        string statusCode,
        string description,
        OperationProcessorContext context
    )
    {
        if (!operation.Responses.ContainsKey(statusCode))
        {
            var response = new OpenApiResponse { Description = description };

            response.Schema = context.SchemaGenerator.Generate<NJsonSchema.JsonSchema>(
                typeof(ErrorResponse),
                context.SchemaResolver
            );
            operation.Responses[statusCode] = response;
        }
    }
}
