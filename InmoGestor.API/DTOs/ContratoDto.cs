#nullable enable
namespace InmoGestor.API.DTOs
{
    public class ContratoResponse
    {
        public string Id { get; set; } = "";
        public string IdPersonaInquilino { get; set; } = "";
        public string Inquilino { get; set; } = "";
        public string DniInquilino { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Inmueble { get; set; } = "";
        public decimal PrecioCuota { get; set; }
        public int CantCuotas { get; set; }
        public string FechaInicio { get; set; } = "";
        public string FechaFin { get; set; } = "";
        public decimal MoraMensual { get; set; }
        public decimal MoraDiaria { get; set; }
        public decimal MoraDiariaMonto { get; set; }
        public int Estado { get; set; }
    }

    public class CrearContratoRequest
    {
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public int CantidadCuotas { get; set; }
        public decimal PrecioCuota { get; set; }
        public decimal TasaMoraMensual { get; set; }
        public string? Condiciones { get; set; }
        public string? InmuebleId { get; set; }
        public string? DniInquilino { get; set; }
        public string? RolInquilinoId { get; set; }
        public string? FrecuenciaAjuste { get; set; }
        public string? IdTipoIndice { get; set; }
        public decimal? ValorIndiceInicio { get; set; }
    }
}
