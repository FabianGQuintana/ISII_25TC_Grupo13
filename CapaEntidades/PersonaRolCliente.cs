using System;

namespace CapaEntidades
{
    public class PersonaRolCliente
    {
        public Guid IdPersona { get; set; }
        public Guid IdRolCliente { get; set; }
        public bool Estado { get; set; }

        public Persona? OPersona { get; set; }
        public RolCliente? ORolCliente { get; set; }
    }
}
