using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Cine.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CarteleraAdminModel : PageModel
{
    private readonly SarmiMovieDbContext _context;

    public CarteleraAdminModel(SarmiMovieDbContext context)
    {
        _context = context;
    }

    public List<Pelicula> Peliculas { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public void OnGet()
    {
        var query = _context.Peliculas
            .Where(p => p.Activa == true); // Solo películas activas

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(p => p.Titulo.Contains(SearchTerm));
        }

        Peliculas = query.ToList();
    }

    // Método POST: desactiva una película (Quitar)
    public async Task<IActionResult> OnPostDesactivarAsync(int id)
    {
        var pelicula = await _context.Peliculas.FindAsync(id);
        if (pelicula != null && pelicula.Activa == true)
        {
            pelicula.Activa = false;
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(); // Refresca la cartelera
    }
}
