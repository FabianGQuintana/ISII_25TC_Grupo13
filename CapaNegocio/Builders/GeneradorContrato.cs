
using CapaEntidades;

namespace CapaNegocio
{
    public class GeneradorContrato //Orquestador de la construcción de un contrato-Director
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
