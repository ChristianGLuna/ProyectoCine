using System;
using System.Collections.Generic;

namespace Proyecto_Cine.Models;

public partial class AsientosFuncion
{
    public int FuncionId { get; set; }

    public string Asiento { get; set; } = null!;

    public bool? Disponible { get; set; }

    public virtual Funcione Funcion { get; set; } = null!;
}
