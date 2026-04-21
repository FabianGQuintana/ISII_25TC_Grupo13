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

        /// <summary>
        /// Punto de entrada principal para el controller.
        /// Devuelve el valor vigente del índice:
        ///   1. Busca en caché (TTL según tipo: diario ICL, mensual IPC).
        ///   2. Si no hay caché, obtiene el valor de la API externa.
        ///   3. Guarda en DB y retorna.
        /// </summary>
        public async Task<HistoricoIndice> ObtenerOActualizar(Guid idTipoIndice)
        {
            var tipo = ResolverTipo(idTipoIndice)
                ?? throw new Exception("Tipo de índice no encontrado.");

            // 1. Caché vigente
            var cached = _capaDato.ObtenerActual(idTipoIndice, tipo.Nombre);
            if (cached != null)
                return cached;

            // 2. Fetch externo
            var valor = await FetchValorExterno(tipo.Nombre);

            // 3. Guardar y retornar
            var nuevo = new HistoricoIndice
            {
                IdHistoricoIndice = Guid.NewGuid(),
                IdTipoIndice = idTipoIndice,
                Valor = valor,
                FechaValidez = DateTime.Now
            };

            _capaDato.InsertarHistorico(nuevo, tipo.Nombre);
            return nuevo;
        }

        /// <summary>
        /// Guarda un valor manualmente (por compatibilidad con el endpoint POST).
        /// </summary>
        public void GuardarHistorico(HistoricoIndice obj)
        {
            if (obj.Valor <= 0)
                throw new Exception("El valor del índice no puede ser nulo o negativo.");
            if (obj.IdTipoIndice == Guid.Empty)
                throw new Exception("El tipo de índice es obligatorio.");

            var nombre = ResolverTipo(obj.IdTipoIndice)?.Nombre ?? string.Empty;
            _capaDato.InsertarHistorico(obj, nombre);
        }

        // ── Privados ──────────────────────────────────────────────────────────

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
