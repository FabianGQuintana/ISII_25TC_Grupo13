using System;

namespace CapaEntidades
{
    public class HistoricoIndice
    {
        public Guid IdHistoricoIndice { get; set; }
        public Guid IdTipoIndice { get; set; }
        public decimal Valor { get; set; }
        public DateTime FechaValidez { get; set; }
    }
}
