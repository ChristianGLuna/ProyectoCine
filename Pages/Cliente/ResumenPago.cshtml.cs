using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto_Cine.Models;

namespace Proyecto_Cine.Pages.Cliente
{
    public class ResumenPagoModel : PageModel
    {
        private readonly SarmiMovieDbContext _context;

        public ResumenPagoModel(SarmiMovieDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int idFuncion { get; set; }

        public Funcione? Funcion { get; set; }
        public Pelicula? Pelicula { get; set; }
        public string NombreSucursal { get; set; } = string.Empty;
        public string NombreSala { get; set; } = string.Empty;

        public string FechaFormateada => Funcion?.Fecha?.ToString("dd/MM/yyyy") ?? "";
        public string HoraFormateada => Funcion?.HoraInicio?.ToString("HH:mm") ?? "";
        public string BoletosDescripcion { get; set; } = "3 adultos, 1 niño(s)";
        public string AsientosSeleccionados { get; set; } = "G1, G2, G3, G4";
        public decimal Total { get; set; } = 277.00M;

        public async Task<IActionResult> OnGetAsync()
        {
            Funcion = await _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sucursal)
                .FirstOrDefaultAsync(f => f.Id == idFuncion);

            if (Funcion == null)
                return NotFound();

            Pelicula = Funcion.Pelicula;
            NombreSucursal = Funcion.Sucursal?.Nombre ?? "Desconocido";
            NombreSala = Funcion.Sala?.Nombre ?? "Desconocido";

            return Page();
        }
    }
}
