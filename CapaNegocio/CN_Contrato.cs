using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Contrato
    {
        private readonly ICD_Contrato _cdContrato;
        private readonly CN_Cuota _cnCuota;
        private readonly CN_Inmueble _cnInmueble;

        public CN_Contrato()
            : this(new CD_Contrato(), new CN_Cuota(), new CN_Inmueble())
        {
        }

        public CN_Contrato(ICD_Contrato cdContrato, CN_Cuota cnCuota, CN_Inmueble cnInmueble)
        {
            _cdContrato = cdContrato;
            _cnCuota = cnCuota;
            _cnInmueble = cnInmueble;
        }

        public const int MaxCantidadCuotas = 120;

        private static readonly string[] FrecuenciasAjusteValidas =
            { "Cuatrimestral", "Semestral", "Anual" };

        public List<ContratoAlquiler> Listar(int? estado = null)
        {
            return _cdContrato.Listar(estado);
        }

        public List<ContratoAlquiler> ListarActivosPorInquilino(Guid idInquilino)
        {
            if (idInquilino == Guid.Empty)
                throw new ArgumentException("El ID del inquilino es requerido");
            return _cdContrato.ListarActivosPorInquilino(idInquilino);
        }

        public ContratoAlquiler? ObtenerPorId(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _cdContrato.ObtenerPorId(id);
        }

        public (bool success, string message, Guid? contratoId) CrearContrato(ContratoAlquiler contrato)
        {
            if (contrato == null)
                return (false, "El contrato es requerido", null);

            if (contrato.IdInmueble == Guid.Empty)
                return (false, "El inmueble es requerido", null);

            if (contrato.IdPersonaInquilino == Guid.Empty)
                return (false, "El inquilino es requerido", null);

            if (contrato.CantidadCuotas <= 0)
                return (false, "La cantidad de cuotas debe ser mayor a 0", null);

            if (contrato.CantidadCuotas > MaxCantidadCuotas)
                return (false, $"La cantidad de cuotas no puede superar {MaxCantidadCuotas}", null);

            if (contrato.PrecioCuota <= 0)
                return (false, "El precio de la cuota debe ser mayor a 0", null);

            if (contrato.TasaMoraMensual < 0)
                return (false, "La tasa de mora no puede ser negativa", null);

            if (!string.IsNullOrWhiteSpace(contrato.FrecuenciaAjuste)
                && Array.IndexOf(FrecuenciasAjusteValidas, contrato.FrecuenciaAjuste) < 0)
                return (false, "La frecuencia de ajuste no es válida", null);

            if (contrato.IdTipoIndice.HasValue && contrato.IdTipoIndice.Value != Guid.Empty
                && !contrato.ValorIndiceInicio.HasValue)
                return (false, "Debe ingresar el valor del índice seleccionado", null);

            if (contrato.FechaFin == default)
                return (false, "La fecha de fin es requerida", null);

            if (contrato.FechaFin <= contrato.FechaCreacion)
                return (false, "La fecha de fin debe ser posterior a la fecha de inicio", null);

            var (disponible, mensajeDisponible) = _cdContrato.ValidarInmuebleDisponible(contrato.IdInmueble);
            if (!disponible)
                return (false, mensajeDisponible, null);

            if (contrato.IdRolClienteInquilino == Guid.Empty)
            {
                var idRol = _cdContrato.ObtenerIdRolInquilino(contrato.IdPersonaInquilino);
                if (idRol == null)
                    return (false, "El inquilino seleccionado no tiene un rol 'Inquilino' válido asignado en el sistema.", null);

                contrato.IdRolClienteInquilino = idRol.Value;
            }

            var builder = new ContratoAlquilerBuilder();
            var director = new GeneradorContrato();
            var contratoFinal = director.Construir(builder, contrato);

            return _cdContrato.CrearContrato(contratoFinal);
        }

        public (bool success, string message) Rescindir(Guid idContrato)
        {
            if (idContrato == Guid.Empty)
                return (false, "El ID del contrato es requerido");

            var contrato = _cdContrato.ObtenerPorId(idContrato);
            if (contrato == null)
                return (false, "Contrato no encontrado");

            if (contrato.Estado != "Activo")
                return (false, "El contrato no está activo");

            var (success, message) = _cdContrato.RescindirEstado(idContrato);
            if (!success)
                return (false, message);

            _cnCuota.CambiarEstadoCuotas(idContrato);
            _cnInmueble.LiberarInmueble(idContrato);

            return (true, "Contrato rescindido exitosamente");
        }

        public (bool success, string message) ValidarInmuebleDisponible(Guid idInmueble, Guid? idContratoExcluir = null)
        {
            if (idInmueble == Guid.Empty)
                return (false, "El inmueble es requerido");

            return _cdContrato.ValidarInmuebleDisponible(idInmueble, idContratoExcluir);
        }
    }
}
