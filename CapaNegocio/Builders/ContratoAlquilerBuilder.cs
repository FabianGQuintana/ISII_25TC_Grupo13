using System;
using CapaEntidades;

namespace CapaNegocio
{
    public class ContratoAlquilerBuilder : IContratoBuilder
    {
        private readonly ContratoAlquiler _contrato = new();
        private readonly CN_Cuota _cnCuota = new();

        public IContratoBuilder ConDatos(ContratoAlquiler datos)
        {
            _contrato.IdContratoAlquiler = Guid.NewGuid();
            _contrato.FechaCreacion = datos.FechaCreacion;
            _contrato.FechaFin = datos.FechaFin;
            _contrato.CantidadCuotas = datos.CantidadCuotas;
            _contrato.PrecioCuota = datos.PrecioCuota;
            _contrato.Condiciones = datos.Condiciones;
            _contrato.IdInmueble = datos.IdInmueble;
            _contrato.IdPersonaInquilino = datos.IdPersonaInquilino;
            _contrato.IdRolClienteInquilino = datos.IdRolClienteInquilino;
            _contrato.IdUsuarioCreador = datos.IdUsuarioCreador;
            return this;
        }

        public IContratoBuilder ConAjuste(string? frecuenciaAjuste, Guid? idTipoIndice, decimal tasaMoraMensual, decimal? valorIndiceInicio)
        {
            _contrato.FrecuenciaAjuste = frecuenciaAjuste;
            _contrato.IdTipoIndice = idTipoIndice;
            _contrato.TasaMoraMensual = tasaMoraMensual;
            _contrato.ValorIndiceInicio = valorIndiceInicio;
            return this;
        }

        public IContratoBuilder GenerarCuotas()
        {
            _contrato.Cuotas = _cnCuota.GenerarParaContrato(_contrato);
            return this;
        }

        public ContratoAlquiler Build() => _contrato;
    }
}
