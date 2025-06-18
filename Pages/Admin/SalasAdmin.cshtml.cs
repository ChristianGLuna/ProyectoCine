using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto_Cine.Models;

namespace Proyecto_Cine.Pages.Admin
{
    public class SalasAdminModel : PageModel
    {
        private readonly SarmiMovieDbContext _context;

        public SalasAdminModel(SarmiMovieDbContext context)
        {
            _context = context;
        }

        public List<Sala> Salas { get; set; } = new();

        public void OnGet()
        {
            Salas = _context.Salas
                .Include(s => s.IdSucursalNavigation)
                .ToList();
        }

        public IActionResult OnPostSuspender(int id)
        {
            var sala = _context.Salas.FirstOrDefault(s => s.Id == id);
            if (sala != null)
            {
                // sala.Activa = false;
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
    }
}
