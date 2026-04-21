using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Indice
    {
        // Obtiene el registro si fue creado HOY, según la sugerencia de volatilidad del ICL
        public HistoricoIndice? ObtenerActual(Guid idTipoIndice)
        {
            HistoricoIndice? obj = null;

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT TOP 1 id_historico_indice, id_tipo_indice, valor, fecha_validez 
                    FROM historico_indice 
                    WHERE id_tipo_indice = @idTipoIndice 
                    AND CAST(fecha_validez AS DATE) = CAST(GETDATE() AS DATE) 
                    ORDER BY fecha_validez DESC";

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
            }

            return obj;
        }

        public List<TipoIndice> ListarTipos()
        {
            List<TipoIndice> lista = new List<TipoIndice>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = "SELECT id_tipo_indice, nombre, descripcion FROM tipo_indice";

                using (var cmd = new SqlCommand(query, cn))
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
            }

            return lista;
        }

        public void InsertarHistorico(HistoricoIndice obj)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    INSERT INTO historico_indice (id_historico_indice, id_tipo_indice, valor, fecha_validez)
                    VALUES (@id, @idTipo, @valor, @fecha)";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", obj.IdHistoricoIndice == Guid.Empty ? Guid.NewGuid() : obj.IdHistoricoIndice);
                    cmd.Parameters.AddWithValue("@idTipo", obj.IdTipoIndice);
                    cmd.Parameters.AddWithValue("@valor", obj.Valor);
                    // Asegurarnos de usar GETDATE de SQL o la fecha pasada
                    cmd.Parameters.AddWithValue("@fecha", obj.FechaValidez == DateTime.MinValue ? DateTime.Now : obj.FechaValidez);
                    
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
