using CapaDatos;
using CapaEntidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class CN_CuotaAdicional
    {
        private readonly CD_CuotaAdicional _cd = new();

        public bool InsertarCuotaAdicional(CuotaAdicional adicional)
        {
            return _cd.InsertarCuotaAdicional(adicional);
        }
    }
}
