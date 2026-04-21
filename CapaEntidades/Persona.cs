using System;

namespace CapaEntidades
{
    public class Persona
    {
        public Guid IdPersona { get; set; }
        public string? Dni { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Estado { get; set; } = "Activo";
        public DateTime? FechaNacimiento { get; set; }
        
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}