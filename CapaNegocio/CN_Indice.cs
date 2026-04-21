using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Indice
    {
        private CD_Indice objCapaDato = new CD_Indice();

        public List<TipoIndice> ListarTipos()
        {
            return objCapaDato.ListarTipos();
        }

        public HistoricoIndice? ObtenerActual(Guid idTipoIndice)
        {
            return objCapaDato.ObtenerActual(idTipoIndice);
        }

        public void GuardarHistorico(HistoricoIndice obj)
        {
            // Validaciones mínimas de negocio
            if (obj.Valor <= 0)
            {
                throw new Exception("El valor del índice no puede ser nulo o negativo.");
            }
            if (obj.IdTipoIndice == Guid.Empty)
            {
                throw new Exception("El tipo de índice es obligatorio.");
            }

            objCapaDato.InsertarHistorico(obj);
        }
    }
}
