#nullable enable
namespace InmoGestor.API.DTOs
{
    public class CrearCuotaAdicionalRequest
    {
        public string IdCuota { get; set; } = "";

        public string IdTipoAdicionales { get; set; } = "";

        public decimal? MontoAplicado { get; set; }

        public string? DescripcionManual { get; set; }
    }
}
