using CapaNegocio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
             .AllowAnyHeader()
             .AllowAnyMethod();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La configuración 'Jwt:Key' es requerida. Definila en appsettings.json o como variable de entorno.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "InmoGestor";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddScoped<CN_Usuario>();
builder.Services.AddScoped<CN_Contrato>();
builder.Services.AddScoped<CN_Inquilino>();
builder.Services.AddScoped<CN_Inmueble>();
builder.Services.AddScoped<CN_Pago>();
builder.Services.AddScoped<CN_Persona>();
builder.Services.AddScoped<CN_Indice>();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error != null)
        {
            var isDev = app.Environment.EnvironmentName == "Development";
            var mensaje = isDev ? feature.Error.Message : "Ocurrió un error interno del servidor.";
            await context.Response.WriteAsJsonAsync(new { success = false, mensaje });
        }
    });
});

if (app.Environment.EnvironmentName == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();