using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto_Cine.Models;
using Proyecto_Cine.Models.ViewModels;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Proyecto_Cine.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SarmiMovieDbContext _context;

        public LoginModel(SarmiMovieDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LoginViewModel LoginData { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            string correo = LoginData.Correo.Trim().ToLower();
            string hash = CalcularHash(LoginData.Contrasena);

            // LOG temporal
            Console.WriteLine($"[LOGIN] Correo: {correo}");
            Console.WriteLine($"[LOGIN] Hash generado: {hash}");


            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Email!.ToLower() == correo &&
                    u.ContrasenaHash == hash &&
                    u.Activo == true);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                return Page();
            }

            // Crear claims de identidad
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Firmar la cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirigir según rol
            return usuario.Rol == "Admin"
                ? RedirectToPage("/PanelAdmin")
                : RedirectToPage("/Home");
        }

        private string CalcularHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
