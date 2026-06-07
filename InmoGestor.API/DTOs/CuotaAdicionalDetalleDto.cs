namespace InmoGestor.API.DTOs
{
    public class CuotaAdicionalDetalleDto
    {
        public string TipoAdicional { get; set; } = "";
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = "";
    }
}
