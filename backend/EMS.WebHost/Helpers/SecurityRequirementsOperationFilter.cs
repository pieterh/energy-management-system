using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EMS.WebHost.Helpers;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    /// <summary>
    /// Applies OpenAPI operation filter to document security requirements.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to modify.</param>
    /// <param name="context">The current operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        var controllerAttributes = context.MethodInfo?.DeclaringType?.GetCustomAttributes(true) ?? Array.Empty<object>();
        var methodAttributes = context.MethodInfo?.GetCustomAttributes(true) ?? Array.Empty<object>();

        if (!HasAnonymousOnMethod(methodAttributes))
        {
            var authorizationPoliciesOnController = GetAuthorizationAttributes(controllerAttributes);
            var authorizationPoliciesOnMethod = GetAuthorizationPoliciesOnMethod(methodAttributes);

            if (authorizationPoliciesOnController.Any() || authorizationPoliciesOnMethod.Any())
            {
                AddSecurityRequirements(operation, authorizationPoliciesOnMethod);
            }
        }
    }

    private static void AddSecurityRequirements(OpenApiOperation operation, IEnumerable<string> authorizationPoliciesOnMethod)
    {
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ scheme ] = authorizationPoliciesOnMethod.ToList()
                    }
                };
    }

    private static bool HasAnonymousOnMethod(IEnumerable<object> customAttributes)
    {
        return customAttributes.OfType<AllowAnonymousAttribute>().Any();
    }

    private static IEnumerable<AuthorizeAttribute> GetAuthorizationAttributes(IEnumerable<object> customAttributes)
    {
        return customAttributes?.OfType<AuthorizeAttribute>().Distinct() ?? Array.Empty<AuthorizeAttribute>();
    }

    private static IEnumerable<string> GetAuthorizationPoliciesOnMethod(IEnumerable<object> customAttributes)
    {
        return customAttributes
                   .OfType<AuthorizeAttribute>()
                   .Select(attr => attr.Policy ?? string.Empty)
                   .Where(policy => !string.IsNullOrWhiteSpace(policy))
                   .Distinct();
    }
}
