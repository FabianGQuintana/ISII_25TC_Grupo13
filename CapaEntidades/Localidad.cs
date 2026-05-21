using System;

namespace CapaEntidades
{
    public class Localidad
    {
        public Guid IdLocalidad { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public Guid IdProvincia { get; set; }
        public Provincia? OProvincia { get; set; }
    }
}
