using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Localidad
    {
        public List<Localidad> ListarLocalidades()
        {
            return Listar(null);
        }

        public List<Localidad> ListarPorProvincia(Guid idProvincia)
        {
            return Listar(idProvincia);
        }

        private List<Localidad> Listar(Guid? idProvincia)
        {
            var lista = new List<Localidad>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT l.id_localidad, l.nombre, l.id_provincia, p.nombre AS provincia_nombre
                    FROM localidad l
                    INNER JOIN provincia p ON l.id_provincia = p.id_provincia";

                if (idProvincia.HasValue)
                    query += " WHERE l.id_provincia = @idProvincia";

                query += " ORDER BY l.nombre";

                using (var cmd = new SqlCommand(query, cn))
                {
                    if (idProvincia.HasValue)
                        cmd.Parameters.AddWithValue("@idProvincia", idProvincia.Value);

                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Localidad
                            {
                                IdLocalidad = Guid.Parse(dr["id_localidad"].ToString()!),
                                Nombre = dr["nombre"]?.ToString() ?? "",
                                IdProvincia = Guid.Parse(dr["id_provincia"].ToString()!),
                                OProvincia = new Provincia
                                {
                                    IdProvincia = Guid.Parse(dr["id_provincia"].ToString()!),
                                    Nombre = dr["provincia_nombre"]?.ToString() ?? ""
                                }
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}
