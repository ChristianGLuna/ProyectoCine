using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Cine.Models;

namespace Proyecto_Cine.Pages.Admin
{
    public class ModificarAgregarSalasAdminModel : PageModel
    {
        private readonly SarmiMovieDbContext _context;

        public ModificarAgregarSalasAdminModel(SarmiMovieDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Sala Sala { get; set; } = new();

        public List<Sucursale> Sucursales { get; set; } = new();

        public IActionResult OnGet(int? id)
        {
            Sucursales = _context.Sucursales.ToList();

            if (id != null)
            {
                Sala = _context.Salas.FirstOrDefault(s => s.Id == id)!;
                if (Sala == null) return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            if (Sala.Id == 0)
                _context.Salas.Add(Sala);
            else
                _context.Salas.Update(Sala);

            _context.SaveChanges();
            return RedirectToPage("/Admin/SalasAdmin");
        }
    }
}
