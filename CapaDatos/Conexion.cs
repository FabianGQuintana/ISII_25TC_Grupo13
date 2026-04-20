using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace CapaDatos
{
    public static class Conexion
    {
        private static string? _cadena;
        
        public static string Cadena
        {
            get
            {
                if (string.IsNullOrEmpty(_cadena))
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    
                    var config = builder.Build();
                    _cadena = config.GetConnectionString("cadena_conexion");
                }
                return _cadena ?? string.Empty;
            }
            set => _cadena = value;
        }
    }
}