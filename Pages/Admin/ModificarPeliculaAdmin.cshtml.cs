using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto_Cine.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Proyecto_Cine.Pages.Admin;

public class ModificarPeliculaAdminModel : PageModel
{
    private readonly SarmiMovieDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ModificarPeliculaAdminModel(SarmiMovieDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [BindProperty]
    public Pelicula Pelicula { get; set; } = new();

    [BindProperty]
    public IFormFile? ArchivoImagen { get; set; }  // Para recibir la imagen

    public bool EsNueva => Pelicula.Id == 0;

    public IActionResult OnGet(int? id)
    {
        if (id == null)
        {
            Pelicula = new Pelicula { Activa = true };
        }
        else
        {
            Pelicula = _context.Peliculas.FirstOrDefault(p => p.Id == id);
            if (Pelicula == null)
                return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        string? rutaImagenNueva = null;

        if (ArchivoImagen != null)
        {
            string nombreArchivo = Path.GetFileName(ArchivoImagen.FileName);
            string rutaDestino = Path.Combine(_env.WebRootPath, "images", nombreArchivo);

            using (var stream = new FileStream(rutaDestino, FileMode.Create))
            {
                ArchivoImagen.CopyTo(stream);
            }

            rutaImagenNueva = "/images/" + nombreArchivo;
        }

        if (Pelicula.Id == 0)
        {
            Pelicula.Activa = true;
            if (rutaImagenNueva != null)
                Pelicula.Imagen = rutaImagenNueva;

            _context.Peliculas.Add(Pelicula);
        }
        else
        {
            var existente = _context.Peliculas.FirstOrDefault(p => p.Id == Pelicula.Id);
            if (existente == null) return NotFound();

            existente.Titulo = Pelicula.Titulo;
            existente.Genero = Pelicula.Genero;
            existente.Duracion = Pelicula.Duracion;
            existente.Clasificacion = Pelicula.Clasificacion;
            existente.Idioma = Pelicula.Idioma;
            existente.Sinopsis = Pelicula.Sinopsis;
            existente.Activa = Pelicula.Activa;

            // Solo cambia la imagen si subieron una nueva
            if (rutaImagenNueva != null)
                existente.Imagen = rutaImagenNueva;
        }

        _context.SaveChanges();
        return RedirectToPage("CarteleraAdmin");
    }
}
