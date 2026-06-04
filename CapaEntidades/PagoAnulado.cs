using System;

namespace CapaEntidades
{
    public class PagoAnulado
    {
        public Guid IdPagoAnulado { get; set; }
        public Guid IdPago { get; set; }
        public DateTime FechaAnulacion { get; set; }
        public Guid IdUsuarioAnulacion { get; set; }
        public decimal MontoTotalAnulado { get; set; }
        public string? Motivo { get; set; }
    }
}
