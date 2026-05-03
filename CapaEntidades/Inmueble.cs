using System;

namespace CapaEntidades
{
    public class Inmueble
    {
        public int Id { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = "Activo";
        public int Disponibilidad { get; set; }
        public string? DisponibilidadNombre { get; set; }
        public string? PropietarioNombreCompleto { get; set; }
        public string? TipoNombre { get; set; }

        public Guid IdInmueble 
        { 
            get => Guid.NewGuid(); 
            set { } 
        }
    }
}