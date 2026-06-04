using System;

namespace CapaEntidades
{
    public abstract class PersonaRolCliente
    {
        public Guid IdPersona { get; set; }
        public Guid IdRolCliente { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }

        public Persona? OPersona { get; set; }
        public RolCliente? ORolCliente { get; set; }
    }
}
