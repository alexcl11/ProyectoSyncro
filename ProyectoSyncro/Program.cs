using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Azure;
using ProyectoSyncro.Policies;
using ProyectoSyncro.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
});
string keyVaultUrl = builder.Configuration["KeyVault:VaultUri"]; // Asegúrate de que esta ruta coincida con tu appsettings.json
SecretClient clienteSecreto = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());


// Extraemos la URL base de tu API en Azure
var apiBaseUrl = new Uri(builder.Configuration["ApiConfig:BaseUrl"]);

// Registramos los 4 servicios independientes
builder.Services.AddHttpClient<AuthApiService>(client => client.BaseAddress = apiBaseUrl);
builder.Services.AddHttpClient<BaseApiService>(client => client.BaseAddress = apiBaseUrl);
builder.Services.AddHttpClient<SettingsApiService>(client => client.BaseAddress = apiBaseUrl);

// El de IA no usa BaseAddress porque llama a n8n, no a tu API
builder.Services.AddHttpClient<AiApiService>();

builder.Services.AddTransient<IAuthorizationHandler, FreeTierTableLimitHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LimitesFreeTablas", policy =>
        policy.Requirements.Add(new FreeTierRequirement()));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = "/Auth/Login"; });

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

KeyVaultSecret stripeSecretKey = await clienteSecreto.GetSecretAsync("stripe-secretkey");
string secretKeyStripe = stripeSecretKey.Value;
StripeConfiguration.ApiKey = secretKeyStripe;

var app = builder.Build();
app.UseStaticFiles();
app.UseSession(); // 👈 IMPORTANTE: Antes de Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();