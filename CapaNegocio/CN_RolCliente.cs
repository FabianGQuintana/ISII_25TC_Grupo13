
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_RolCliente
    {
        private readonly CD_RolCliente _cdRolCliente = new();

        public List<RolCliente> ListarRoles()
        {
            return _cdRolCliente.ListarRoles();
        }
    }
}
