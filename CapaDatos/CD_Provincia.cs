using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Provincia
    {
        public List<Provincia> ListarProvincias()
        {
            var lista = new List<Provincia>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = "SELECT id_provincia, nombre FROM provincia ORDER BY nombre";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Provincia
                            {
                                IdProvincia = Guid.Parse(dr["id_provincia"].ToString()!),
                                Nombre = dr["nombre"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}
