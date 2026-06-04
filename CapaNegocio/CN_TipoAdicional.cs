using CapaDatos;
using CapaEntidades;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_TipoAdicional
    {
        private readonly CD_TipoAdicional _cd = new();

        public List<TipoAdicional> ListarAdicionales()
        {
            return _cd.ListarAdicionales();
        }
    }
}