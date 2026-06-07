#nullable enable
namespace InmoGestor.API.DTOs
{
    public class InmuebleResponse
    {
        public string Id { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Disponibilidad { get; set; }
        public string DisponibilidadNombre { get; set; } = "";
        public string PropietarioNombreCompleto { get; set; } = "";
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? IdLocalidad { get; set; }
        public string? IdProvincia { get; set; }
    }
}
