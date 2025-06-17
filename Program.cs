using Microsoft.EntityFrameworkCore;
using Proyecto_Cine.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 🔌 Conexión a base de datos MySQL
builder.Services.AddDbContext<SarmiMovieDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// ✅ AGREGAR servicios ANTES de builder.Build()
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 🌐 Configurar el pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Asegúrate de tener esto si usas wwwroot

app.UseRouting();

app.UseAuthentication(); // Importante: primero autenticación
app.UseAuthorization();

// 📄 Rutas Razor Pages
app.MapRazorPages();

app.Run();
