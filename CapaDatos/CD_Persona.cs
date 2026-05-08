using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Persona
    {
        public List<Persona> ObtenerTodos()
        {
            var lista = new List<Persona>();
            
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT dni, nombre, apellido, email, telefono, estado, fecha_nacimiento
                    FROM persona
                    WHERE estado = 1";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Persona
                            {
                                Dni = dr["dni"].ToString()!,
                                Nombre = dr["nombre"].ToString()!,
                                Apellido = dr["apellido"].ToString()!,
                                Email = dr["email"] != DBNull.Value ? dr["email"].ToString()! : "",
                                Telefono = dr["telefono"] == DBNull.Value ? null : dr["telefono"].ToString(),
                                Estado = dr["estado"].ToString()!,
                                FechaNacimiento = dr["fecha_nacimiento"] == DBNull.Value ? null : DateTime.Parse(dr["fecha_nacimiento"].ToString()!)
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}