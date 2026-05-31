using CapaDatos;
using CapaEntidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class CN_Pago
    {
        private readonly CD_Pago _cdPago = new();
        private readonly CN_Cuota _cnCuota = new();

        public (bool Success, string Message) ProcesarAnulacionPago(Guid idPago, string motivo, Guid idUsuario)
        {
            var pago = _cdPago.ObtenerPorId(idPago);
            if (pago == null)
                return (false, "Pago no encontrado");

            if (pago.Estado == "Anulado")
                return (false, "El pago ya se encuentra anulado");

            var pagoAnulado = new PagoAnulado
            {
                IdPagoAnulado = Guid.NewGuid(),
                IdPago = idPago,
                FechaAnulacion = DateTime.Now,
                IdUsuarioAnulacion = idUsuario,
                MontoTotalAnulado = pago.MontoTotal,
                Motivo = motivo
            };

            var resultado = _cdPago.CrearPagoAnulado(pagoAnulado);
            return resultado
                ? (true, "Pago anulado exitosamente")
                : (false, "Error al anular el pago");
        }

        public async Task<(bool success, string message)> RegistrarPago(
             Guid idCuota,
             Guid idMetodoPago,
             Guid idUsuario,
             bool esSuperior)
        {
            var cuota = new CD_Cuota().ObtenerPorId(idCuota);

            if (cuota == null)
            {
                return (false, "No se encontró la cuota");
            }

            var cuotaCalculada =
                await _cnCuota.ObtenerCuotaCalculada(cuota.IdContratoAlquiler);

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
                    ? (true, "Pago registrado correctamente")
                    : (false, "Error al procesar");
            }

            return (true, "Pago registrado correctamente");
        }

        public List<Pago> Listar(int? estado)
        {
            return _cdPago.Listar(estado.HasValue ? estado.Value.ToString() : null);
        }

        public List<Pago> ListarPorContrato(Guid contratoId)
        {
            return _cdPago.ListarPorContrato(contratoId);
        }

        public async Task<(CuotaCalculadaDto? detalle, List<MetodoPago> metodosPago)> MostrarDetallePago(Guid idContrato)
        {
            var detalle = await _cnCuota.ObtenerCuotaCalculada(idContrato);
            var metodos = _cdPago.ListarMetodosPagos();
            return (detalle, metodos);
        }

        public List<MetodoPago> ListarMetodosPagos()
        {
            return _cdPago.ListarMetodosPagos();
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