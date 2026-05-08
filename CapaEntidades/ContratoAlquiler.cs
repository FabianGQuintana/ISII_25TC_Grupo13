using System;

namespace CapaEntidades
{
    public class ContratoAlquiler
    {
        public Guid IdContratoAlquiler { get; set; }
        public DateTime FechaFin { get; set; }
        public string? Condiciones { get; set; }
        public int CantidadCuotas { get; set; }
        public decimal PrecioCuota { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Guid IdInmueble { get; set; }
        public Guid IdPersonaInquilino { get; set; }
        public Guid IdRolClienteInquilino { get; set; }
        public decimal TasaMoraMensual { get; set; }
        public string Estado { get; set; } = "Activo";
        public Guid IdUsuarioCreador { get; set; }
        public string? FrecuenciaAjuste { get; set; }
        public Guid? IdTipoIndice { get; set; }
        public decimal? ValorIndiceInicio { get; set; }

        public decimal MoraDiaria => TasaMoraMensual / 30m;
        public decimal MoraDiariaMonto => PrecioCuota * MoraDiaria / 100m;

        public Persona? OInquilino { get; set; }
        public Inmueble? OInmueble { get; set; }
    }
}