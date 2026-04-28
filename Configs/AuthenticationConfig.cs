using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
namespace swagger_cognito_ex.Configs;

public static class AuthenticationConfig
{
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var cognitoSettings = configuration.GetSection("Cognito");
        var authority = cognitoSettings["Authority"];
        var audience = cognitoSettings["Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();

                    if (string.IsNullOrEmpty(authHeader))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var errorResponse = new
                        {
                            error = "Unauthorized",
                            details = "Authentication is required to access this resource"
                        };
                        context.Response.WriteAsJsonAsync(errorResponse).Wait();
                        context.HandleResponse();
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var exception = context.Exception;
                    Console.WriteLine($"Authentication failed: {exception?.GetType().Name}");
                    Console.WriteLine($"Exception message: {exception?.Message}");
                    Console.WriteLine($"Inner exception: {exception?.InnerException?.Message}");
                    // 400 - Token malformat
                    if (exception is ArgumentException ||
                        exception is SecurityTokenMalformedException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        var errorResponse = new
                        {
                            error = "Malformed token",
                            details = "The provided token is not in a valid format"
                        };
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(errorResponse);
                    }

                    // 401 - Token invalid sau expirat
                    if (exception is SecurityTokenExpiredException)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        var errorResponse = new
                        {
                            error = "Token expired",
                            details = "The access token has expired"
                        };
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(errorResponse);
                    }

                    // 401 - Generic invalid token
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        error = "Invalid token",
                        details = "The provided token is invalid or not recognized by Cognito"
                    };
                    return context.Response.WriteAsJsonAsync(response);
                },
                OnTokenValidated = context =>
                {
                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    Console.WriteLine($"Token validated successfully for user: {userId}");
                    return Task.CompletedTask;
                }
            };
        });
    }
}
