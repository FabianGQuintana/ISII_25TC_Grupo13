using System.Collections.Generic;

namespace InmoGestor.API.DTOs
{
    public class DetallePagoResponse
    {
        public CuotaCalculadaDto Cuota { get; set; } = null!;
        public List<MetodoPagoDto> MetodosPago { get; set; } = new();
    }

    public class MetodoPagoDto
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
    }
}
