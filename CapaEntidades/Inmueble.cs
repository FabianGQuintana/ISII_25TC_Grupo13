using System;

namespace CapaEntidades
{
    public class Inmueble
    {
        public Guid IdInmueble { get; set; }
        public Guid IdDireccion { get; set; }
        public string? Descripcion { get; set; }
        public string Estado { get; set; } = "Activo";
        public DateTime FechaCreacion { get; set; }
        public Guid IdPersonaPropietario { get; set; }
        public Guid IdRolClientePropietario { get; set; }
        public byte[]? Imagen { get; set; }
        public bool Disponibilidad { get; set; }
        public Guid? IdTipoInmueble { get; set; }

        public Direccion? ODireccion { get; set; }
        public Persona? OPropietario { get; set; }
    }
}