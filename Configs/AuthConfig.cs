using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace swagger_cognito_ex.Configs;

public static class AuthConfig
{
    public static IServiceCollection AddCognitoAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["AWS:Cognito:Authority"];
        var clientId = configuration["AWS:Cognito:ClientId"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };
            });

        return services;
    }

}
