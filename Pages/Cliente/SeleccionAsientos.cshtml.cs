using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto_Cine.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public Funcione? Funcion { get; set; }
        public Pelicula? Pelicula { get; set; }
        public string NombreSucursal { get; set; } = string.Empty;
        public string NombreSala { get; set; } = string.Empty;

        public string FechaFormateada => Funcion?.Fecha?.ToString("dd/MM/yyyy") ?? "";
        public string HoraFormateada => Funcion?.HoraInicio?.ToString("HH:mm") ?? "";

        public Dictionary<string, List<AsientoVista>> MatrizAsientos { get; set; } = new();

        [BindProperty]
        public List<int> AsientosSeleccionados { get; set; } = new();

        [BindProperty]
        public string BoletosDescripcion { get; set; } = "";

        [BindProperty]
        public decimal Total { get; set; }

        [BindProperty]
        public string BoletosJson { get; set; } = "";


        public class AsientoVista
        {
            public int Id { get; set; }
            public string Fila { get; set; } = string.Empty;
            public int Numero { get; set; }
            public bool Disponible { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Recuperar datos desde TempData para mantenerlos entre páginas
            if (TempData.ContainsKey("BoletosDescripcion"))
                BoletosDescripcion = TempData["BoletosDescripcion"]?.ToString() ?? "";

            if (TempData.ContainsKey("Total") && decimal.TryParse(TempData["Total"]?.ToString(), out decimal totalTemp))
                Total = totalTemp;

            if (TempData.ContainsKey("AsientosSeleccionados"))
            {
                var asientosStr = TempData["AsientosSeleccionados"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(asientosStr))
                {
                    AsientosSeleccionados = asientosStr.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(s => int.TryParse(s, out int id) ? id : 0)
                                                       .Where(id => id > 0)
                                                       .ToList();
                }
            }

            await CargarInformacion();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
{
    if (AsientosSeleccionados == null || !AsientosSeleccionados.Any())
    {
        ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un asiento.");
        await CargarInformacion();
        return Page();
    }

    await CargarInformacion();

    // 🔸 Deserializar el JSON de boletos
    var datosBoletos = new List<(int tipo, int cantidad)>();

    if (!string.IsNullOrEmpty(BoletosJson))
    {
        try
        {
            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, int>>>(BoletosJson);

            if (parsed != null)
            {
                datosBoletos = parsed
                    .Where(p => p.ContainsKey("tipo") && p.ContainsKey("cantidad"))
                    .Select(p => (tipo: p["tipo"], cantidad: p["cantidad"]))
                    .ToList();
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Error al interpretar la selección de boletos.");
            await CargarInformacion();
            return Page();
        }
    }

    // 🔸 Consultar precios reales desde la BD
    var preciosBD = await _context.PreciosFuncions
        .Where(pf => pf.FuncionId == idFuncion)
        .ToDictionaryAsync(pf => pf.TipoBoletoId, pf => pf.Precio);

    // 🔸 Cargar nombres desde TiposBoleto
    var nombresBoletos = await _context.TiposBoletos
        .ToDictionaryAsync(tb => tb.Id, tb => tb.Nombre);

    decimal total = 0;
    var partesDescripcion = new List<string>();

    foreach (var item in datosBoletos)
{
    if (!preciosBD.TryGetValue(item.tipo, out decimal? precioNullable) || precioNullable == null)
        continue;

    decimal precioUnitario = precioNullable.Value;
    var subtotal = precioUnitario * item.cantidad;
    total += subtotal;

    var nombre = nombresBoletos.ContainsKey(item.tipo) ? nombresBoletos[item.tipo] : $"Tipo {item.tipo}";
    partesDescripcion.Add($"{item.cantidad} {nombre}(s)");
}

    BoletosDescripcion = string.Join(", ", partesDescripcion);
    Total = total;

    // 🔸 Guardar en TempData para pasar a ResumenPago
    TempData["FuncionId"] = idFuncion;
    TempData["AsientosSeleccionados"] = string.Join(",", AsientosSeleccionados);
    TempData["BoletosDescripcion"] = BoletosDescripcion;
    TempData["Total"] = Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

    return RedirectToPage("/Cliente/ResumenPago", new { idFuncion = idFuncion });
}


        private async Task CargarInformacion()
        {
            var funcion = await _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                .Include(f => f.Sucursal)
                .FirstOrDefaultAsync(f => f.Id == idFuncion);

            if (funcion == null)
                return;

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
                af => af.Disponible ?? true
            );

            MatrizAsientos.Clear();

            foreach (var asiento in asientosSala)
            {
                var fila = asiento.Fila ?? "???";
                var numero = asiento.Numero ?? 0;

                if (!MatrizAsientos.ContainsKey(fila))
                    MatrizAsientos[fila] = new List<AsientoVista>();

                bool disponible = mapaDisponibilidad.TryGetValue(asiento.Id, out var estado)
                    ? estado
                    : true;

                MatrizAsientos[fila].Add(new AsientoVista
                {
                    Id = asiento.Id,
                    Fila = fila,
                    Numero = numero,
                    Disponible = disponible
                });
            }
        }
    }
}
