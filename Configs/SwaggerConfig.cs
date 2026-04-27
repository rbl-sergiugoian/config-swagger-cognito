using System.Security.Cryptography.X509Certificates;
using Microsoft.OpenApi;

namespace swagger_cognito_ex.Configs;

public static class SwaggerConfig
{
    public static void AddSwaggerWithCognito(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["AWS:Cognito:Authority"];
        var domain = configuration["AWS:Cognito:Domain"];

        if (authority == null)
        {
            throw new UnauthorizedAccessException("authority not found");
        }

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Cognito swagger api",
                Version = "v1"
            });

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{domain}/oauth2/authorize"),
                        TokenUrl = new Uri($"{domain}/oauth2/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" },
                            { "profile", "Profile" }
                        }
                    }
                }
            });

            options.AddSecurityRequirement(doc =>
            {
                var securityRequirement = new OpenApiSecurityRequirement();

                securityRequirement.Add(
                    new OpenApiSecuritySchemeReference("oauth2", doc),
                    new List<string> { "openid", "profile" }
                );

                return securityRequirement;
            });

        });
    }

    public static WebApplication UseSwaggerWithCognito(this WebApplication app, IConfiguration configuration)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API");

            options.OAuthClientId(configuration["AWS:Cognito:ClientId"]);
            options.OAuthUsePkce();
            options.OAuthScopeSeparator(" ");
            options.OAuthAppName("Swagger API");

            options.EnablePersistAuthorization();
        });

        return app;
    }
}
