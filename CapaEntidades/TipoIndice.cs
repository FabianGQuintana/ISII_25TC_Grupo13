using System;

namespace CapaEntidades
{
    public class TipoIndice
    {
        public Guid IdTipoIndice { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}