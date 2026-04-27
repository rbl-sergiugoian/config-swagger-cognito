using Microsoft.OpenApi;

namespace swagger_cognito_ex.Configs;

public static class SwaggerConfig
{
    public static void AddSwaggerWithCognito(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Cognito swagger api",
                Version = "v1"
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
