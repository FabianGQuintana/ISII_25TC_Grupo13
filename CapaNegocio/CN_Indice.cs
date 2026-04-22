using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Indice
    {
        private readonly CD_Indice _capaDato = new CD_Indice();

        private const string URL_IPC = "https://api.argly.com.ar/api/ipc";
        private const string URL_ICL = "https://api.argly.com.ar/api/icl";

        public List<TipoIndice> ListarTipos()
        {
            return _capaDato.ListarTipos();
        }
        public async Task<HistoricoIndice> ObtenerOActualizar(Guid idTipoIndice)
        {
            var tipo = ResolverTipo(idTipoIndice)
                ?? throw new Exception("Tipo de índice no encontrado.");

            // 1. ¿Ya consultamos HOY? (Caché diaria para no saturar Argly)
            var hoy = _capaDato.ObtenerActual(idTipoIndice);
            if (hoy != null)
                return hoy;

            // 2. Caché miss de hoy: Traemos el valor de la API externa
            var valorExterno = await FetchValorExterno(tipo.Nombre);

            // 3. Guardar el registro de hoy
            var nuevo = new HistoricoIndice
            {
                IdHistoricoIndice = Guid.NewGuid(),
                IdTipoIndice = idTipoIndice,
                Valor = valorExterno,
                FechaValidez = DateTime.Now
            };

            _capaDato.InsertarHistorico(nuevo);
            return nuevo;
        }

        public void GuardarHistorico(HistoricoIndice obj)
        {
            if (obj.Valor <= 0)
                throw new Exception("El valor del índice no puede ser nulo o negativo.");
            if (obj.IdTipoIndice == Guid.Empty)
                throw new Exception("El tipo de índice es obligatorio.");

            _capaDato.InsertarHistorico(obj);
        }

        private TipoIndice? ResolverTipo(Guid idTipoIndice)
            => _capaDato.ListarTipos().FirstOrDefault(t => t.IdTipoIndice == idTipoIndice);

        private async Task<decimal> FetchValorExterno(string nombreTipo)
        {
            bool esIPC = nombreTipo.Equals("IPC", StringComparison.OrdinalIgnoreCase);
            bool esICL = nombreTipo.Equals("ICL", StringComparison.OrdinalIgnoreCase);

            if (!esIPC && !esICL)
                throw new Exception($"No hay fuente externa configurada para el índice '{nombreTipo}'.");

            var url = esIPC ? URL_IPC : URL_ICL;

            using var http = new HttpClient();
            var json = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");
            var prop = esIPC ? data.GetProperty("indice_ipc") : data.GetProperty("valor");

            return prop.ValueKind == JsonValueKind.String
                ? decimal.Parse(prop.GetString()!.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture)
                : prop.GetDecimal();
        }
    }
}
