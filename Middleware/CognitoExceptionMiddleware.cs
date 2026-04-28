namespace swagger_cognito_ex.Middleware;

public class CognitoExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public CognitoExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("Cognito") || ex.Message.Contains("cognito"))
        {
            Console.WriteLine("Cognito service communication error");
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var errorResponse = new
                {
                    error = "Cognito unavailable",
                    details = "Unable to reach Cognito service. Please try again later."
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Unable to obtain configuration"))
        {
            Console.WriteLine("Unable to obtain configuration");
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var errorResponse = new
                {
                    error = "Authentication service unavailable",
                    details = "Cognito configuration cannot be loaded. Please try again later."
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error in authentication middleware");

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var errorResponse = new
                {
                    error = "Internal server error",
                    details = "An unexpected error occurred"
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
