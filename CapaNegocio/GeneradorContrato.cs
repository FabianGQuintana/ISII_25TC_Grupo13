
using CapaEntidades;

namespace CapaNegocio
{
    public class GeneradorContrato
    {
        public ContratoAlquiler Construir(IContratoBuilder builder, ContratoAlquiler datos)
        {
            return builder
                .ConDatos(datos)
                .ConAjuste(datos.FrecuenciaAjuste, datos.IdTipoIndice, datos.TasaMoraMensual, datos.ValorIndiceInicio)
                .GenerarCuotas()
                .Build();
        }
    }
}
