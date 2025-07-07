using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto_Cine.Models;
using System.Globalization;

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
    public string NombreSucursal { get; set; } = "";
    public string NombreSala { get; set; } = "";
    public string FechaFormateada => Funcion?.Fecha?.ToString("dd/MM/yyyy") ?? "";
    public string HoraFormateada => Funcion?.HoraInicio?.ToString("HH:mm") ?? "";

    [BindProperty]
    public string BoletosDescripcionInput { get; set; } = "";

    [BindProperty]
    public string AsientosSeleccionados { get; set; } = "";

    public string AsientosSeleccionadosTexto { get; set; } = "";

    [BindProperty]
    public string MetodoPago { get; set; } = "";

    [BindProperty]
    public string TotalInput { get; set; } = "";

    public decimal TotalDecimal => decimal.TryParse(TotalInput, out var t) ? t : 0;

    public Dictionary<string, (int Cantidad, decimal PrecioUnitario, decimal Subtotal)> DesgloseBoletos { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await CargarInformacion();

        if (TempData.ContainsKey("BoletosDescripcion"))
        {
            BoletosDescripcionInput = TempData["BoletosDescripcion"]?.ToString() ?? "";
            TempData.Keep("BoletosDescripcion");
        }

        if (TempData.ContainsKey("AsientosSeleccionados"))
        {
            AsientosSeleccionados = TempData["AsientosSeleccionados"]?.ToString() ?? "";
            TempData.Keep("AsientosSeleccionados");
        }

        if (!string.IsNullOrEmpty(AsientosSeleccionados))
            AsientosSeleccionadosTexto = await ObtenerTextoAsientos(AsientosSeleccionados);

        if (TempData.ContainsKey("Total"))
        {
            var rawTotal = TempData["Total"]?.ToString()?.Replace(",", ".");

            if (decimal.TryParse(rawTotal, NumberStyles.Any, CultureInfo.InvariantCulture, out var totalParsed))
            {
                TotalInput = totalParsed.ToString("F2", CultureInfo.InvariantCulture);
            }
            else
            {
                TotalInput = "0.00";
            }

            TempData.Keep("Total");
        }

        await CalcularDesgloseBoletos();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarInformacion();

        if (!string.IsNullOrWhiteSpace(AsientosSeleccionados))
            AsientosSeleccionadosTexto = await ObtenerTextoAsientos(AsientosSeleccionados);

        if (string.IsNullOrWhiteSpace(BoletosDescripcionInput) || string.IsNullOrWhiteSpace(TotalInput))
        {
            ModelState.AddModelError(string.Empty, "Faltan datos de los boletos o el total.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(MetodoPago))
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar un método de pago.");
            return Page();
        }

        var idAsientos = AsientosSeleccionados
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(idStr => int.TryParse(idStr, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToList();

        foreach (var idAsiento in idAsientos)
        {
            var asientoFuncion = await _context.AsientosFuncions
                .FirstOrDefaultAsync(af => af.FuncionId == idFuncion && af.AsientoId == idAsiento);

            if (asientoFuncion != null)
                asientoFuncion.Disponible = false;
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("/Cliente/Confirmacion");
    }

    private async Task CargarInformacion()
    {
        Funcion = await _context.Funciones
            .Include(f => f.Pelicula)
            .Include(f => f.Sala)
            .Include(f => f.Sucursal)
            .FirstOrDefaultAsync(f => f.Id == idFuncion);

        if (Funcion != null)
        {
            Pelicula = Funcion.Pelicula;
            NombreSucursal = Funcion.Sucursal?.Nombre ?? "Desconocido";
            NombreSala = Funcion.Sala?.Nombre ?? "Desconocido";
        }
    }

    private async Task<string> ObtenerTextoAsientos(string idsCsv)
    {
        var ids = idsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => int.TryParse(id, out var num) ? num : 0)
            .Where(id => id > 0)
            .ToList();

        var asientos = await _context.Asientos
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();

        var codigos = asientos
            .OrderBy(a => a.Fila)
            .ThenBy(a => a.Numero)
            .Select(a => $"{a.Fila}{a.Numero}");

        return string.Join(", ", codigos);
    }

    private async Task CalcularDesgloseBoletos()
{
    if (string.IsNullOrWhiteSpace(BoletosDescripcionInput) || Funcion == null)
        return;

    // Paso 1: Obtener todos los precios de la función actual
    var preciosFuncion = await _context.PreciosFuncions
        .Where(p => p.FuncionId == Funcion.Id)
        .ToDictionaryAsync(p => p.TipoBoletoId, p => p.Precio ?? 0m);

    // Paso 2: Mapeo de nombre a tipo_boleto_id (ajusta si usas otros nombres)
    var tipoMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "Adulto", 1 },
        { "Niño", 2 },
        { "Senior", 3 }
    };

    // Paso 3: Parsear la cadena tipo "2 Adulto(s), 1 Niño(s)"
    var tipos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    foreach (var parte in BoletosDescripcionInput.Split(','))
    {
        var limpio = parte.Trim();
        if (string.IsNullOrWhiteSpace(limpio)) continue;

        var split = limpio.Split(' ');
        if (split.Length >= 2 && int.TryParse(split[0], out int cantidad))
        {
            var tipo = split[1].Replace("(s)", "").Trim();
            tipos[tipo] = cantidad;
        }
    }

    // Paso 4: Calcular desglose por tipo de boleto
    DesgloseBoletos.Clear();

    foreach (var kv in tipos)
    {
        if (tipoMap.TryGetValue(kv.Key, out int tipoId) &&
            preciosFuncion.TryGetValue(tipoId, out decimal precio))
        {
            var subtotal = precio * kv.Value;
            DesgloseBoletos[kv.Key] = (kv.Value, precio, subtotal);
        }
    }
}

}

}
