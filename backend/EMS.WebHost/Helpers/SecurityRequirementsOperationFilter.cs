using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EMS.WebHost.Helpers
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(context);

            var hasAnonymousOnMethod = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Distinct()
                .Any();

            if (!hasAnonymousOnMethod)
            {
                var authorizationPoliciesOnController = context.MethodInfo.DeclaringType
                    ?.GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Distinct() ?? Array.Empty<AuthorizeAttribute>();

                var authorizationPoliciesOnMethod = context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Select(attr => attr.Policy)
                    .Distinct() ?? Array.Empty<string>();

                if (authorizationPoliciesOnController.Any() || authorizationPoliciesOnMethod.Any())
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
            }
        }
    }
}

