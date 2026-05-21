using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class CuotaAdicional
    {
        public Guid IdCuotaAdicional { get; set; }

        public Guid IdCuota { get; set; }

        public Guid IdTipoAdicionales { get; set; }

        public decimal MontoAplicado { get; set; }

        public string? DescripcionManual { get; set; }

        public DateTime FechaCreacion { get; set; }

        public TipoAdicional? OTipoAdicional { get; set; }
    }
}
