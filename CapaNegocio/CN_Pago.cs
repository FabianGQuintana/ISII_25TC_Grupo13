using CapaDatos;
using CapaEntidades;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Pago
    {
        private readonly CD_Pago _cdPago = new();

        public (bool success, string message) RegistrarPago(Pago pago, bool esSuperior)
        {
            // Si no es superior, el pago queda pendiente
            pago.Estado = esSuperior ? "Aprobado" : "Pendiente";

            _cdPago.Insertar(pago);

            // Si ya era superior, aprobamos inmediatamente el flujo
            if (esSuperior)
            {
                return (_cdPago.AprobarPago(pago.IdPago, pago.IdCuota) ? (true, "Pago aprobado y registrado") : (false, "Error al procesar"));
            }

            return (true, "Pago registrado, esperando aprobación de un superior.");
        }

        public List<Pago> Listar(int? estado)
        {
            return _cdPago.Listar(estado);
        }

        public List<Pago> ListarPorContrato(Guid contratoId)
        {
            return _cdPago.ListarPorContrato(contratoId);
        }

        public List<ContratoAlquiler> ListarActivosPorInquilino(Guid idInquilino)
        {
            return _cdContrato.ListarActivosPorInquilino(idInquilino);
        }

        public bool Rechazar(Guid idPago, string? motivo)
        {
            return _cdPago.Rechazar(idPago, motivo);
        }

        public bool Anular(Guid idPago)
        {
            return _cdPago.Anular(idPago);
        }
        public Pago ObtenerPorId(Guid id)
        {
            
            return _cdPago.ObtenerPorId(id);
        }

        public bool AprobarPago(Guid idPago, Guid idCuota)
        {
            return _cdPago.AprobarPago(idPago, idCuota);
        }
    }
}