using System;

namespace CapaEntidades
{
    public class Cuota
    {
        public Guid IdCuota { get; set; }
        public Guid IdContratoAlquiler { get; set; }
        public int NroCuota { get; set; }
        public string Periodo { get; set; } = "";
        public DateTime FechaVencimiento { get; set; }
        public decimal ImporteBase { get; set; }
        public string Estado { get; set; } = "";
        public decimal ValorMoraAplicada { get; set; }
        public decimal DescuentoAdicionalTotal { get; set; }
        public decimal OtrosAdicionalesTotal { get; set; }
        public decimal ValorIndiceAplicado { get; set; }
        public decimal ImporteTotalCalculado { get; set; }
    }
}
