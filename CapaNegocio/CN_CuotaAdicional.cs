using CapaDatos;
using CapaEntidades;
using System;

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
