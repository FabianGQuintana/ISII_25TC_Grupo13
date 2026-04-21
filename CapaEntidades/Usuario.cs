using System;

namespace CapaEntidades
{
    public class Usuario
    {
        public Guid IdUsuario { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Estado { get; set; } = "Activo";
        public DateTime FechaCreacion { get; set; }
        
        public Guid IdPersona { get; set; }
        public Guid IdRolUsuario { get; set; }
        
        public Persona? OPersona { get; set; }
        public RolUsuario? ORolUsuario { get; set; }
        
        public string NombreCompleto => OPersona?.NombreCompleto ?? string.Empty;
        public string Email => OPersona?.Email ?? string.Empty;
    }
}