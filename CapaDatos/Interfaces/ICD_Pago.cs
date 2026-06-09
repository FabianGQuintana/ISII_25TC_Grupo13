using CapaEntidades;
using System;
using System.Collections.Generic;

namespace CapaDatos
{
    public interface ICD_Pago
    {
        void Insertar(Pago pago);
        Pago? ObtenerPorId(Guid id);
        bool CrearPagoAnulado(PagoAnulado entidad);
        List<Pago> Listar(string? estado);
        List<Pago> ListarPorContrato(Guid contratoId);
        List<MetodoPago> ListarMetodosPagos();
        bool Rechazar(Guid idPago, string? motivo);
        bool Anular(Pago pago, Guid idUsuario, string? motivo);
        bool AprobarPago(Guid idPago, Guid idCuota);
    }
}
