using System;

namespace CapaEntidades
{
    public class RolCliente
    {
        public Guid IdRolCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
