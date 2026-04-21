using System;

namespace CapaEntidades
{
    public class Direccion
    {
        public Guid IdDireccion { get; set; }
        public string Calle { get; set; } = string.Empty;
        public string Altura { get; set; } = string.Empty;
    }
}