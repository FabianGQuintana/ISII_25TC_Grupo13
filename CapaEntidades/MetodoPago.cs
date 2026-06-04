using System;

namespace CapaEntidades
{
    public class MetodoPago
    {
        public Guid IdMetodoPago { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
