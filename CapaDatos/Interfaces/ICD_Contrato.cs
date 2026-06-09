using System;
using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public interface ICD_Contrato
    {
        List<ContratoAlquiler> Listar(int? estado = null);
        List<ContratoAlquiler> ListarActivosPorInquilino(Guid idInquilino);
        ContratoAlquiler? ObtenerPorId(Guid id);
        Guid? ObtenerIdRolInquilino(Guid idPersona);
        (bool success, string message, Guid? contratoId) CrearContrato(ContratoAlquiler contrato);
        (bool success, string message) RescindirEstado(Guid idContrato);
        (bool success, string message) ValidarInmuebleDisponible(Guid idInmueble, Guid? idContratoExcluir = null);
    }
}
