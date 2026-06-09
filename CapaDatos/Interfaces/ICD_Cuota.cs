using CapaEntidades;
using System;
using System.Collections.Generic;

namespace CapaDatos
{
    public interface ICD_Cuota
    {
        Cuota? ObtenerPorId(Guid idCuota);
        bool MarcarComoPagada(Guid idCuota);
        List<Cuota> ListarPendientesPorContrato(Guid idContrato);
        Cuota? ObtenerUltimaPendientePorContrato(Guid contratoId);
        int AnularPendientesPorContrato(Guid idContrato);
        bool ActualizarDescuento(Guid idCuota, decimal descuento);
    }
}
