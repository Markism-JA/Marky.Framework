using System.Text.Json.Serialization;
using Asp.Versioning;
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
        string apiDescription,
        Action<ApiVersioningOptions>? configureVersioning = null
    )
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                configureVersioning?.Invoke(options);
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
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
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your_jwt_token}.",
                    }
                );

                options.OperationProcessors.Add(new AutoErrorResponseProcessor());
                options.OperationProcessors.Add(new SecurityRequirementsOperationProcessor());

                var fluentValidationSchemaProcessor = serviceProvider
                    .CreateScope()
                    .ServiceProvider.GetRequiredService<FluentValidationSchemaProcessor>();
                options.SchemaSettings.SchemaProcessors.Add(fluentValidationSchemaProcessor);
            }
        );

        return services;
    }
}
