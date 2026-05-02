using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
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

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddScoped<CapaNegocio.CN_Usuario>();
builder.Services.AddScoped<CapaNegocio.CN_Contrato>();
builder.Services.AddScoped<CapaNegocio.CN_Inquilino>();
builder.Services.AddScoped<CapaNegocio.CN_Inmueble>();
builder.Services.AddScoped<CapaNegocio.CN_Indice>();
builder.Services.AddScoped<CapaNegocio.CN_Pago>();
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