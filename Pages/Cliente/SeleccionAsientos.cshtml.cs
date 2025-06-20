using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto_Cine.Models;

namespace Proyecto_Cine.Pages.Cliente
{
    public class SeleccionAsientosModel : PageModel
    {
        private readonly SarmiMovieDbContext _context;

        public SeleccionAsientosModel(SarmiMovieDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int idFuncion { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? boletos { get; set; }  // Aquí viene el JSON desde Funciones.cshtml

        public Funcione? Funcion { get; set; }
        public Pelicula? Pelicula { get; set; }
        public string NombreSucursal { get; set; } = string.Empty;
        public string NombreSala { get; set; } = string.Empty;

        public string FechaFormateada => Funcion?.Fecha?.ToString("dd/MM/yyyy") ?? "";
        public string HoraFormateada => Funcion?.HoraInicio?.ToString("HH:mm") ?? "";

        public Dictionary<string, List<AsientoVista>> MatrizAsientos { get; set; } = new();

        [BindProperty]
        public List<int> AsientosSeleccionados { get; set; } = new();

        public class AsientoVista
        {
            public int Id { get; set; }
            public string Fila { get; set; } = string.Empty;
            public int Numero { get; set; }
            public bool Disponible { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var funcion = await _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                .Include(f => f.Sucursal)
                .FirstOrDefaultAsync(f => f.Id == idFuncion);

            if (funcion == null)
                return NotFound();

            Funcion = funcion;
            Pelicula = funcion.Pelicula;
            NombreSucursal = funcion.Sucursal?.Nombre ?? "Desconocido";
            NombreSala = funcion.Sala?.Nombre ?? "Desconocido";

            var asientosSala = await _context.Asientos
                .Where(a => a.SalaId == funcion.SalaId)
                .ToListAsync();

            var asientosFuncion = await _context.AsientosFuncions
                .Where(af => af.FuncionId == idFuncion)
                .ToListAsync();

            var mapaDisponibilidad = asientosFuncion.ToDictionary(
                af => af.AsientoId,
                af => af.Disponible
            );

            foreach (var asiento in asientosSala)
            {
                var fila = asiento.Fila ?? "???";
                var numero = asiento.Numero ?? 0;

                if (!MatrizAsientos.ContainsKey(fila))
                    MatrizAsientos[fila] = new List<AsientoVista>();

                bool disponible = mapaDisponibilidad.TryGetValue(asiento.Id, out var estado)
                    ? estado ?? true
                    : true;

                MatrizAsientos[fila].Add(new AsientoVista
                {
                    Id = asiento.Id,
                    Fila = fila,
                    Numero = numero,
                    Disponible = disponible
                });
            }

            // Aquí podrías deserializar el JSON de boletos si quieres usarlo
            if (!string.IsNullOrEmpty(boletos))
            {
                // Ejemplo (opcional): var boletosSeleccionados = JsonSerializer.Deserialize<Dictionary<int, int>>(boletos);
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (AsientosSeleccionados == null || !AsientosSeleccionados.Any())
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un asiento.");
                return Page();
            }

            TempData["FuncionId"] = idFuncion;
            TempData["AsientosSeleccionados"] = string.Join(",", AsientosSeleccionados);

            return RedirectToPage("/Cliente/ResumenPago");
        }
    }
}
