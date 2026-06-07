using System;
using System.Collections.Generic;

namespace InmoGestor.API.DTOs
{
    public class CuotaCalculadaDto
    {
        public string IdCuota { get; set; } = "";

        public int NroCuota { get; set; }

        public string Periodo { get; set; } = "";

        public DateTime FechaVencimiento { get; set; }

        public decimal PrecioCuota { get; set; }

        public decimal ValorIndiceAplicado { get; set; }

        public decimal ImporteActualizado { get; set; }

        public decimal TotalAdicionales { get; set; }

        public List<CuotaAdicionalDetalleDto> DetalleAdicionales { get; set; } = new();

        public decimal TotalDescuentos { get; set; }

        public int DiasAtraso { get; set; }

        public decimal MoraCalculada { get; set; }

        public decimal TotalFinal { get; set; }

        public string Estado { get; set; } = "";
    }
}
