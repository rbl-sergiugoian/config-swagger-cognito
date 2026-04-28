using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace swagger_cognito_ex.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult GetProtected()
    {
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;

        return Ok(new
        {
            message = "200 - Token is valid, access granted",
            userId,
            email
        });
    }

    [HttpGet("public")]
    public IActionResult GetPublic()
    {
        return Ok(new { message = "Public endpoint - no auth required" });
    }

    [HttpGet("test-cognito-error")]
    public IActionResult TestCognitoError()
    {
        // Simulează o cerere la Cognito endpoint care nu există
        var httpClient = new HttpClient();
        try
        {
            var result = httpClient.GetAsync("https://cognito-idp.eu-north-1.amazonaws.com/invalid-endpoint").Result;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("Cognito service unreachable", ex);
        }
        return Ok();
    }
}
