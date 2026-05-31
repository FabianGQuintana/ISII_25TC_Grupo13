using CapaEntidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace CapaDatos
{
    public class CD_TipoAdicional
    {
        public List<TipoAdicional> ListarAdicionales()
        {
            var lista = new List<TipoAdicional>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = "SELECT id_tipo_adicionales, descripcion, monto_base FROM tipo_adicionales ORDER BY descripcion";
                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new TipoAdicional
                            {
                                IdTipoAdicionales = dr["id_tipo_adicionales"] is Guid id ? id : Guid.Empty,
                                Descripcion = dr["descripcion"]?.ToString() ?? "",
                                MontoBase = dr["monto_base"] is decimal mb ? mb : 0m
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}