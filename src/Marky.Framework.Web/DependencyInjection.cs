using System.Text.Json.Serialization;
using Marky.Framework.Web.AccessControl;
using MicroElements.NSwag.FluentValidation;
using MicroElements.NSwag.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSwag;

namespace Marky.Framework.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddEnterpriseWebFramework(
        this IServiceCollection services,
        string apiTitle,
        string apiDescription
    )
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddFluentValidationRulesToSwagger();
        services.AddOpenApiDocument(
            (options, serviceProvider) =>
            {
                options.Title = apiTitle;
                options.Version = "v1";
                options.Description = apiDescription;

                options.AddSecurity(
                    "JWT",
                    Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your_jwt_token}.",
                    }
                );
                options.OperationProcessors.Add(new AutoErrorResponseProcessor());

                var fluentValidationSchemaProcessor =
                    serviceProvider.GetRequiredService<FluentValidationSchemaProcessor>();
                options.SchemaSettings.SchemaProcessors.Add(fluentValidationSchemaProcessor);
            }
        );

        return services;
    }
}
