using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoSyncro.Data;
using ProyectoSyncro.Policies;
using ProyectoSyncro.Repositories;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication(options =>
{
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme; 
}).AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
});

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlCRMConnection")));

builder.Services.AddScoped<SettingsRepository>();
builder.Services.AddScoped<BaseRepository>();
builder.Services.AddScoped<AuthRepository>();

builder.Services.AddTransient<IAuthorizationHandler, FreeTierTableLimitHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LimitesFreeTablas", policy =>
        policy.Requirements.Add(new FreeTierRequirement()));
});
builder.Services.AddHttpContextAccessor();
// Configuración de la clave de Stripe (Pega aquí tu clave secreta)
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"]; 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
