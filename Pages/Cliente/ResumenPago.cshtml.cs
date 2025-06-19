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

        // Datos para mostrar
        public Funcione? Funcion { get; set; }
        public Pelicula? Pelicula { get; set; }
        public string NombreSucursal { get; set; } = string.Empty;
        public string NombreSala { get; set; } = string.Empty;

        public string FechaFormateada => Funcion?.Fecha?.ToString("dd/MM/yyyy") ?? "";
        public string HoraFormateada => Funcion?.HoraInicio?.ToString("HH:mm") ?? "";

        public string BoletosDescripcion { get; set; } = "";
        public string AsientosSeleccionadosTexto { get; set; } = "";
        public decimal Total { get; set; }

        // Datos recibidos por POST
        [BindProperty]
        public string MetodoPago { get; set; } = "";

        [BindProperty]
        public string AsientosSeleccionados { get; set; } = "";

        [BindProperty]
        public string BoletosDescripcionInput { get; set; } = "";

        [BindProperty]
        public string TotalInput { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarInformacion();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CargarInformacion();

            // Asignar los valores que vienen del formulario a las propiedades visibles
            BoletosDescripcion = BoletosDescripcionInput;
            AsientosSeleccionadosTexto = AsientosSeleccionados;

            if (decimal.TryParse(TotalInput, out decimal total))
                Total = total;

            // Marcar asientos como no disponibles
            if (!string.IsNullOrWhiteSpace(AsientosSeleccionados))
            {
                var listaAsientos = AsientosSeleccionados.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var asientoCodigo in listaAsientos)
                {
                    var fila = asientoCodigo.Substring(0, 1);
                    var numeroStr = asientoCodigo.Substring(1);
                    if (int.TryParse(numeroStr, out int numero))
                    {
                        var asiento = await _context.Asientos
                            .FirstOrDefaultAsync(a => a.SalaId == Funcion.SalaId && a.Fila == fila && a.Numero == numero);

                        if (asiento != null)
                        {
                            var relacion = await _context.AsientosFuncions
                                .FirstOrDefaultAsync(af => af.FuncionId == Funcion.Id && af.Asiento == fila + numero);

                            if (relacion != null)
                            {
                                relacion.Disponible = false;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            // Redirigir a Confirmación o mostrar mensaje
            return RedirectToPage("/Cliente/Confirmacion");
        }

        private async Task CargarInformacion()
        {
            Funcion = await _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sucursal)
                .Include(f => f.Sala)
                .FirstOrDefaultAsync(f => f.Id == idFuncion);

            if (Funcion == null) return;

            Pelicula = Funcion.Pelicula;
            NombreSucursal = Funcion.Sucursal?.Nombre ?? "Desconocido";
            NombreSala = Funcion.Sala?.Nombre ?? "Desconocido";

            // Si el usuario entra desde GET directo, se pueden usar valores por defecto:
            if (string.IsNullOrWhiteSpace(BoletosDescripcion))
                BoletosDescripcion = "3 adultos, 1 niño(s)";

            if (string.IsNullOrWhiteSpace(AsientosSeleccionadosTexto))
                AsientosSeleccionadosTexto = "G1, G2, G3, G4";

            if (Total == 0)
                Total = 277.00M;
        }
    }
}
