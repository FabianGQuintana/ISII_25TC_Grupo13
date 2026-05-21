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
            var cuotaCalculada =
                _cnCuota.ObtenerCuotaCalculada(idCuota);

            if (cuotaCalculada == null)
            {
                return (false, "No se encontró la cuota");
            }

            var pago = new Pago
            {
                IdPago = Guid.NewGuid(),

                IdCuota = idCuota,

                IdMetodoPago = idMetodoPago,

                IdUsuarioCreador = idUsuario,

                Periodo = cuotaCalculada.Periodo,

                MoraCobrada = cuotaCalculada.MoraCalculada,

                MontoTotal = cuotaCalculada.TotalFinal,

                Estado = esSuperior
                    ? "Aprobado"
                    : "Pendiente"
            };

            _cdPago.Insertar(pago);

            if (esSuperior)
            {
                return _cdPago.AprobarPago(
                    pago.IdPago,
                    pago.IdCuota)
                    ? (true, "Pago aprobado y registrado")
                    : (false, "Error al procesar");
            }

            return (true, "Pago registrado, esperando aprobación.");
        }

        public List<Pago> Listar(int? estado)
        {
            // Convertir el filtro opcional a string para la capa de datos
            return _cdPago.Listar(estado.HasValue ? estado.Value.ToString() : null);
        }

        public List<Pago> ListarPorContrato(Guid contratoId)
        {
            return _cdPago.ListarPorContrato(contratoId);
        }

        public List<ContratoAlquiler> ListarActivosPorInquilino(Guid idInquilino)
        {
            return _cdPago.ListarActivosPorInquilino(idInquilino);
        }

        public bool Rechazar(Guid idPago, string? motivo)
        {
            return _cdPago.Rechazar(idPago, motivo);
        }

        public bool Anular(Pago pago, Guid idUsuario, string? motivo)
        {
            return _cdPago.Anular(pago, idUsuario, motivo);
        }
        public Pago? ObtenerPorId(Guid id)
        {
            
            return _cdPago.ObtenerPorId(id);
        }

        public bool AprobarPago(Guid idPago, Guid idCuota)
        {
            return _cdPago.AprobarPago(idPago, idCuota);
        }
    }
}