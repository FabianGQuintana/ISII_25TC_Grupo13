using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Provincia
    {
        private readonly CD_Provincia _cd = new();

        public List<Provincia> ListarProvincias() => _cd.ListarProvincias();
    }
}
