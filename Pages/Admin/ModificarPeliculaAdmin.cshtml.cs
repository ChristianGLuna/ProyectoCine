using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Cine.Models;

namespace Proyecto_Cine.Pages.Admin;

public class ModificarPeliculaAdminModel : PageModel
{
    private readonly SarmiMovieDbContext _context;

    public ModificarPeliculaAdminModel(SarmiMovieDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Pelicula Pelicula { get; set; } = new();

    public bool EsNueva => Pelicula.Id == 0;

    public IActionResult OnGet(int? id)
    {
        if (id == null)
        {
            Pelicula = new Pelicula();
        }
        else
        {
            Pelicula = _context.Peliculas.FirstOrDefault(p => p.Id == id);
            if (Pelicula == null)
            {
                return NotFound();
            }
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Pelicula.Id == 0)
        {
            _context.Peliculas.Add(Pelicula);
        }
        else
        {
            _context.Peliculas.Update(Pelicula);
        }

        _context.SaveChanges();
        return RedirectToPage("CarteleraAdmin");
    }
}
