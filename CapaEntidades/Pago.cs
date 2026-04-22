using System;

namespace CapaEntidades
{
    public class Pago
    {
        public Guid IdPago { get; set; }
        public Guid IdCuota { get; set; }
        public Guid IdMetodoPago { get; set; }
        public Guid IdUsuarioCreador { get; set; }
        public decimal MontoTotal { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Pendiente";
    }
}