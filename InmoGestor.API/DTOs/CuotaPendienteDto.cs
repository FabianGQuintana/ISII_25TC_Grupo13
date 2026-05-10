using System;

namespace InmoGestor.API.DTOs
{
    public class CuotaPendienteDto
    {
        public string IdCuota { get; set; } = "";
        public int NroCuota { get; set; }
        public string Periodo { get; set; } = "";
        public DateTime FechaVencimiento { get; set; }
        public decimal ImporteBase { get; set; }
        public decimal ValorIndiceAplicado { get; set; }
        public decimal ImporteActualizado { get; set; }
        public int DiasAtraso { get; set; }
        public decimal MoraCalculada { get; set; }
        public decimal TotalFinal { get; set; }
        public string Estado { get; set; } = "";
    }
}
