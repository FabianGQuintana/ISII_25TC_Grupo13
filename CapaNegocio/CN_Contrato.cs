using System;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Contrato
    {
        private readonly CD_Contrato _cdContrato = new();

        public (bool success, ContratoAlquiler? contrato, string message) CrearContrato(
            Guid idInmueble,
            Guid idInquilino,
            Guid idUsuarioCreador,
            Guid idTipoIndice,
            DateTime fechaInicio,
            DateTime fechaFin,
            int cantidadCuotas,
            decimal precioCuota,
            string frecuenciaAjuste,
            decimal tasaMoraMensual)
        {
            if (idInmueble == Guid.Empty)
            {
                return (false, null, "El inmueble es requerido");
            }

            if (idInquilino == Guid.Empty)
            {
                return (false, null, "El inquilino es requerido");
            }

            if (fechaInicio >= fechaFin)
            {
                return (false, null, "La fecha de inicio debe ser anterior a la fecha de fin");
            }

            if (cantidadCuotas <= 0)
            {
                return (false, null, "La cantidad de cuotas debe ser mayor a cero");
            }

            if (precioCuota <= 0)
            {
                return (false, null, "El precio de la cuota debe ser mayor a cero");
            }

            var disponible = _cdContrato.ValidarDisponibilidadInmueble(idInmueble, fechaInicio, fechaFin);
            if (!disponible)
            {
                return (false, null, "El inmueble no está disponible en el periodo seleccionado");
            }

<<<<<<< HEAD
            var indiceActual = _cdContrato.ObtenerValorIndiceActual(idTipoIndice);

=======
>>>>>>> main
            var contrato = new ContratoAlquiler
            {
                IdContrato = Guid.NewGuid(),
                IdInmueble = idInmueble,
                IdInquilino = idInquilino,
                IdUsuarioCreador = idUsuarioCreador,
                IdTipoIndice = idTipoIndice,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                CantidadCuotas = cantidadCuotas,
                PrecioCuota = precioCuota,
                FrecuenciaAjuste = frecuenciaAjuste,
                TasaMoraMensual = tasaMoraMensual,
                Estado = "Activo",
                FechaCreacion = DateTime.Now
            };

            var resultado = _cdContrato.CrearContrato(contrato);

            if (resultado)
            {
                return (true, contrato, "Contrato creado exitosamente");
            }

            return (false, null, "Error al crear el contrato");
        }

        public ContratoAlquiler? ObtenerPorId(Guid idContrato)
        {
            return _cdContrato.ObtenerPorId(idContrato);
        }
    }
}