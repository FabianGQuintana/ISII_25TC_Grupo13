using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Indice
    {
        /// <summary>
        /// Obtiene el valor cacheado vigente según el tipo de índice:
        /// - ICL (BCRA): se actualiza diariamente → TTL = día actual.
        /// - IPC (INDEC): se actualiza mensualmente → TTL = mes y año actual.
        /// </summary>
        public HistoricoIndice? ObtenerActual(Guid idTipoIndice, string nombreTipo)
        {
            HistoricoIndice? obj = null;

            // El IPC del INDEC se publica una vez por mes; el ICL del BCRA se publica cada día hábil.
            bool esIPC = nombreTipo.Equals("IPC", StringComparison.OrdinalIgnoreCase);

            string filtroFecha = esIPC
                ? "MONTH(fecha_validez) = MONTH(GETDATE()) AND YEAR(fecha_validez) = YEAR(GETDATE())"
                : "CAST(fecha_validez AS DATE) = CAST(GETDATE() AS DATE)";

            string query = $@"
                SELECT TOP 1 id_historico_indice, id_tipo_indice, valor, fecha_validez
                FROM historico_indice
                WHERE id_tipo_indice = @idTipoIndice
                AND {filtroFecha}
                ORDER BY fecha_validez DESC";

            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@idTipoIndice", idTipoIndice);
                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        obj = new HistoricoIndice
                        {
                            IdHistoricoIndice = Guid.Parse(dr["id_historico_indice"].ToString()!),
                            IdTipoIndice = Guid.Parse(dr["id_tipo_indice"].ToString()!),
                            Valor = Convert.ToDecimal(dr["valor"]),
                            FechaValidez = Convert.ToDateTime(dr["fecha_validez"])
                        };
                    }
                }
            }

            return obj;
        }

        public List<TipoIndice> ListarTipos()
        {
            var lista = new List<TipoIndice>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand("SELECT id_tipo_indice, nombre, descripcion FROM tipo_indice", cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new TipoIndice
                        {
                            IdTipoIndice = Guid.Parse(dr["id_tipo_indice"].ToString()!),
                            Nombre = dr["nombre"].ToString()!,
                            Descripcion = dr["descripcion"].ToString()!
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Inserta un nuevo valor de caché. Para IPC (mensual) verifica que no exista
        /// ya un registro del mismo mes/año antes de insertar y evitar duplicados.
        /// Para ICL (diario) verifica que no exista del mismo día.
        /// </summary>
        public void InsertarHistorico(HistoricoIndice obj, string nombreTipo)
        {
            bool esIPC = nombreTipo.Equals("IPC", StringComparison.OrdinalIgnoreCase);

            // Guardia anti-duplicados: si ya existe un valor vigente, no insertamos
            var existente = ObtenerActual(obj.IdTipoIndice, nombreTipo);
            if (existente != null)
                return;

            string query = @"
                INSERT INTO historico_indice (id_historico_indice, id_tipo_indice, valor, fecha_validez)
                VALUES (@id, @idTipo, @valor, @fecha)";

            DateTime fechaValidez = obj.FechaValidez == DateTime.MinValue ? DateTime.Now : obj.FechaValidez;

            using (var cn = new SqlConnection(Conexion.Cadena))
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@id", obj.IdHistoricoIndice == Guid.Empty ? Guid.NewGuid() : obj.IdHistoricoIndice);
                cmd.Parameters.AddWithValue("@idTipo", obj.IdTipoIndice);
                cmd.Parameters.AddWithValue("@valor", obj.Valor);
                cmd.Parameters.AddWithValue("@fecha", fechaValidez);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
