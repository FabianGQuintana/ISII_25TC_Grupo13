using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class TipoAdicional
    {
        public Guid IdTipoAdicionales { get; set; }

        public string Descripcion { get; set; } = "";

        public decimal MontoBase { get; set; }
    }
}
