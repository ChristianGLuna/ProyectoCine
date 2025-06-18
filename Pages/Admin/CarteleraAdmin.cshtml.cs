using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Cine.Models;
using System.Collections.Generic;
using System.Linq;

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
        .Where(p => p.Activa == true); // Mostrar solo activas

    if (!string.IsNullOrWhiteSpace(SearchTerm))
    {
        query = query.Where(p => p.Titulo.Contains(SearchTerm));
    }

    Peliculas = query.ToList();
}


    public IActionResult OnPostEliminar(int id)
    {
        var pelicula = _context.Peliculas.Find(id);
        if (pelicula != null)
        {
            _context.Peliculas.Remove(pelicula);
            _context.SaveChanges();
        }

        return RedirectToPage();
    }
}
