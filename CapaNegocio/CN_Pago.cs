using CapaDatos;
using CapaEntidades;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Pago
    {
        private readonly CD_Pago _cdPago = new();
        private readonly CN_Cuota _cnCuota = new();

        public (bool success, string message) RegistrarPago(
             Guid idCuota,
             Guid idMetodoPago,
             Guid idUsuario,
             bool esSuperior)
        {
            if (idCuota == Guid.Empty)
                return (false, "La cuota es requerida");

            if (idMetodoPago == Guid.Empty)
                return (false, "El método de pago es requerido");

            if (idUsuario == Guid.Empty)
                return (false, "El usuario es requerido");

            var cuotaCalculada = _cnCuota.ObtenerCuotaCalculada(idCuota);

            if (cuotaCalculada == null)
                return (false, "No se encontró la cuota");

            var pago = new Pago
            {
                IdPago = Guid.NewGuid(),
                IdCuota = idCuota,
                IdMetodoPago = idMetodoPago,
                IdUsuarioCreador = idUsuario,
                Periodo = cuotaCalculada.Periodo,
                MoraCobrada = cuotaCalculada.MoraCalculada,
                MontoTotal = cuotaCalculada.TotalFinal,
                Estado = esSuperior ? "Aprobado" : "Pendiente"
            };

            _cdPago.Insertar(pago);

            if (esSuperior)
            {
                return _cdPago.AprobarPago(pago.IdPago, pago.IdCuota)
                    ? (true, "Pago aprobado y registrado")
                    : (false, "Error al procesar");
            }

            return (true, "Pago registrado, esperando aprobación.");
        }

        public List<Pago> Listar(int? estado)
        {
            return _cdPago.Listar(estado.HasValue ? estado.Value.ToString() : null);
        }

        public List<Pago> ListarPorContrato(Guid contratoId)
        {
            if (contratoId == Guid.Empty)
                return new List<Pago>();

            return _cdPago.ListarPorContrato(contratoId);
        }

        public List<ContratoAlquiler> ListarActivosPorInquilino(Guid idInquilino)
        {
            if (idInquilino == Guid.Empty)
                return new List<ContratoAlquiler>();

            return _cdPago.ListarActivosPorInquilino(idInquilino);
        }

        public bool Rechazar(Guid idPago, string? motivo)
        {
            if (idPago == Guid.Empty)
                return false;

            return _cdPago.Rechazar(idPago, motivo);
        }

        public bool Anular(Pago pago, Guid idUsuario, string? motivo)
        {
            if (pago == null || pago.IdPago == Guid.Empty)
                return false;

            if (idUsuario == Guid.Empty)
                return false;

            return _cdPago.Anular(pago, idUsuario, motivo);
        }

        public Pago? ObtenerPorId(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _cdPago.ObtenerPorId(id);
        }

        public bool AprobarPago(Guid idPago, Guid idCuota)
        {
            if (idPago == Guid.Empty || idCuota == Guid.Empty)
                return false;

            return _cdPago.AprobarPago(idPago, idCuota);
        }
    }
}
