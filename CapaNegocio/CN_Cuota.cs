using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Cuota
    {
        private readonly CD_Cuota _cdCuota = new();

        public List<Cuota> ListarPendientesPorContrato(Guid idContrato)
        {
            return _cdCuota.ListarPendientesPorContrato(idContrato);
        }
    }
}
