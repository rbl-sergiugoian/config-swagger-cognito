using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using swagger_cognito_ex.Configs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerWithCognito(builder.Configuration);

//builder.Services.AddCognitoAuthentication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithCognito(builder.Configuration);
}

app.UseHttpsRedirection();


app.MapControllers();

app.Run();
