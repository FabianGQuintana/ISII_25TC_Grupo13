namespace InmoGestor.API.DTOs
{
    public class ResultadoDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public object? Data { get; set; }
    }
}
