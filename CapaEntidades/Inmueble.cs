using System;

namespace CapaEntidades
{
    public class Inmueble
    {
        public Guid IdInmueble { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = "Activo";
    }
}