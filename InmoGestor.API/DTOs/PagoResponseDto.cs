namespace InmoGestor.API.DTOs
{
    using System;

    public class PagoResponseDto
    {
        public string Id { get; set; } = "";
        public string ContratoId { get; set; } = "";
        public string CuotaId { get; set; } = "";
        public string Inquilino { get; set; } = "";
        public string Inmueble { get; set; } = "";
        public int NroCuota { get; set; }
        public string Periodo { get; set; } = "";
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal Monto { get; set; }
        public decimal Mora { get; set; }
        public int DiasAtraso { get; set; }
        public decimal TotalPagado { get; set; }
        public string Estado { get; set; } = "";
        public string EstadoTexto { get; set; } = "";
    }
}
