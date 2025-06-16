using System;
using System.Collections.Generic;

namespace Proyecto_Cine.Models;

public partial class Funcione
{
    public int Id { get; set; }

    public int? PeliculaId { get; set; }

    public int? SalaId { get; set; }

    public int? SucursalId { get; set; }

    public DateOnly? Fecha { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public decimal? Precio { get; set; }

    public string? Idioma { get; set; }

    public string? Formato { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<AsientosFuncion> AsientosFuncions { get; set; } = new List<AsientosFuncion>();

    public virtual Pelicula? Pelicula { get; set; }

    public virtual ICollection<PreciosFuncion> PreciosFuncions { get; set; } = new List<PreciosFuncion>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual Sala? Sala { get; set; }

    public virtual Sucursale? Sucursal { get; set; }
}
