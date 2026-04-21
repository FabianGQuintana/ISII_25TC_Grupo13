using System;

namespace CapaEntidades
{
    public class ContratoAlquiler
    {
        public Guid IdContrato { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int CantidadCuotas { get; set; }
        public decimal PrecioCuota { get; set; }
        public string FrecuenciaAjuste { get; set; } = string.Empty;
        public decimal TasaMoraMensual { get; set; }
        public string Estado { get; set; } = "Activo";
        public DateTime FechaCreacion { get; set; }

        public Guid IdInmueble { get; set; }
        public Guid IdInquilino { get; set; }
        public Guid IdUsuarioCreador { get; set; }
        public Guid IdTipoIndice { get; set; }

        public Inmueble? OInmueble { get; set; }
        public Persona? OInquilino { get; set; }
        public Usuario? OUsuarioCreador { get; set; }
        public TipoIndice? OTipoIndice { get; set; }

        public decimal MontoTotal => PrecioCuota * CantidadCuotas;
    }
}