using System;
using CapaEntidades;

namespace CapaNegocio
{
    public interface IContratoBuilder
    {
        IContratoBuilder ConDatos(ContratoAlquiler datos);
        IContratoBuilder ConAjuste(string? frecuenciaAjuste, Guid? idTipoIndice, decimal tasaMoraMensual, decimal? valorIndiceInicio);
        IContratoBuilder GenerarCuotas();
        ContratoAlquiler Build();
    }
}
