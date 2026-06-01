using CapaDatos;
using CapaEntidades;
using System;

namespace CapaNegocio
{
    public class CN_CuotaAdicional
    {
        private readonly CD_CuotaAdicional _cd = new();

        public bool Insertar(CuotaAdicional adicional)
        {
            if (adicional == null)
                return false;

            if (adicional.IdCuota == Guid.Empty)
                return false;

            if (adicional.IdTipoAdicionales == Guid.Empty)
                return false;

            if (adicional.MontoAplicado <= 0)
                return false;

            return _cd.Insertar(adicional);
        }
    }
}
