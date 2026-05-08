using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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
        public HistoricoIndice? ObtenerActual(Guid idTipoIndice)
        {
            HistoricoIndice? objeto = null;
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                // Filtro unificado: siempre buscamos si ya existe un registro del DÍA DE HOY.
                string query = @"
                    SELECT TOP 1 id_historico_indice, id_tipo_indice, valor, fecha_validez 
                    FROM historico_indice 
                    WHERE id_tipo_indice = @id 
                      AND CAST(fecha_validez AS DATE) = CAST(GETDATE() AS DATE)
                    ORDER BY fecha_validez DESC";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", idTipoIndice);
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            objeto = new HistoricoIndice
                            {
                                IdHistoricoIndice = Guid.Parse(dr["id_historico_indice"].ToString()!),
                                IdTipoIndice = Guid.Parse(dr["id_tipo_indice"].ToString()!),
                                Valor = Convert.ToDecimal(dr["valor"]),
                                FechaValidez = Convert.ToDateTime(dr["fecha_validez"])
                            };
                        }
                    }
                }
            }
            return objeto;
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

        /// </summary>
        public bool InsertarHistorico(HistoricoIndice obj)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    INSERT INTO historico_indice (id_historico_indice, id_tipo_indice, valor, fecha_validez) 
                    VALUES (@idH, @idT, @val, @fecha)";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@idH", obj.IdHistoricoIndice);
                    cmd.Parameters.AddWithValue("@idT", obj.IdTipoIndice);
                    cmd.Parameters.AddWithValue("@val", obj.Valor);
                    cmd.Parameters.AddWithValue("@fecha", obj.FechaValidez);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }
    }
}
