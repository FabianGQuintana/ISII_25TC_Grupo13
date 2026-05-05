using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Contrato
    {
        private readonly CD_Contrato _cdContrato = new();

        public List<ContratoAlquiler> Listar(int? estado = null)
        {
            return _cdContrato.Listar(estado);
        }

        public ContratoAlquiler? ObtenerPorId(Guid id)
        {
            return _cdContrato.ObtenerPorId(id);
        }

     

        public (bool success, string message, Guid? contratoId) CrearContrato(ContratoAlquiler contrato)
        {
            if (contrato.IdInmueble == Guid.Empty)
            {
                return (false, "El inmueble es requerido", null);
            }

            if (contrato.IdPersonaInquilino == Guid.Empty)
            {
                return (false, "El inquilino es requerido", null);
            }

            if (contrato.CantidadCuotas <= 0)
            {
                return (false, "La cantidad de cuotas debe ser mayor a 0", null);
            }

            if (contrato.PrecioCuota <= 0)
            {
                return (false, "El precio de la cuota debe ser mayor a 0", null);
            }

            if (contrato.FechaFin == default)
            {
                return (false, "La fecha de fin es requerida", null);
            }

            if (contrato.FechaFin <= contrato.FechaInicio)
            {
                return (false, "La fecha de fin debe ser posterior a la fecha de inicio", null);
            }

            return _cdContrato.Insertar(contrato);
        }

        public (bool success, string message) Rescindir(Guid id)
        {
            if (id == Guid.Empty)
            {
                return (false, "El ID del contrato es requerido");
            }

            return _cdContrato.Rescindir(id);
        }

        public (bool success, string message) ValidarInmuebleDisponible(Guid idInmueble, Guid? idContratoExcluir = null)
        {
            return _cdContrato.ValidarInmuebleDisponible(idInmueble, idContratoExcluir);
        }
    }
}