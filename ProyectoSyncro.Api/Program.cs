using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using ProyectoSyncro.Api.Data;
using ProyectoSyncro.Api.Policies;
using ProyectoSyncro.Api.Repositories;
using ProyectoSyncro.Models;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpContextAccessor();
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
});
string keyVaultUrl = builder.Configuration["KeyVault:VaultUri"]; // Asegúrate de que esta ruta coincida con tu appsettings.json
SecretClient clienteSecreto = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

KeyVaultSecret sqlconnectionsecret = await clienteSecreto.GetSecretAsync("sql-secret");
string connectionString = sqlconnectionsecret.Value;

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

KeyVaultSecret n8nSecret = await clienteSecreto.GetSecretAsync("n8nconfig-secret");

// Lo registras en el contenedor de dependencias
builder.Services.AddSingleton(new N8nConfig
{
    ApiKey = n8nSecret.Value
});

builder.Services.AddScoped<SettingsRepository>();
builder.Services.AddScoped<BaseRepository>();
builder.Services.AddScoped<AuthRepository>();
// Registramos el handler de la política
builder.Services.AddScoped<IAuthorizationHandler, FreeTierTableLimitHandler>();

// Creamos la política en sí
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("FreeTierLimit", policy =>
        policy.Requirements.Add(new FreeTierRequirement()));
});
KeyVaultSecret jwtsecretkey = await clienteSecreto.GetSecretAsync("jwt-secretkey");
string secretkey = jwtsecretkey.Value;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey))
        };
    });

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var app = builder.Build();



app.MapOpenApi(); 
app.MapScalarApiReference();
app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
