using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Inmueble
    {
        public List<Inmueble> ObtenerTodos()
        {
            var lista = new List<Inmueble>();
            
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        i.id_inmueble, 
                        i.id_direccion,
                        i.descripcion, 
                        i.estado,
                        i.disponibilidad,
                        d.nombre AS disponibilidad_nombre,
                        p.nombre AS propietario_nombre,
                        p.apellido AS propietario_apellido,
                        t.descripcion AS tipo_nombre
                    FROM inmueble i
                    LEFT JOIN disponibilidad d ON i.disponibilidad = d.id_disponibilidad
                    LEFT JOIN persona p ON i.id_persona_propietario = p.dni
                    LEFT JOIN tipo_inmueble t ON i.id_tipo_inmueble = t.id_tipo_inmueble
                    WHERE i.estado = 1";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var nombreProp = dr["propietario_nombre"] != DBNull.Value ? dr["propietario_nombre"].ToString() : "";
                            var apeProp = dr["propietario_apellido"] != DBNull.Value ? dr["propietario_apellido"].ToString() : "";
                            
                            lista.Add(new Inmueble
                            {
                                Id = Convert.ToInt32(dr["id_inmueble"]),
                                Direccion = "Dirección " + dr["id_direccion"].ToString(),
                                Descripcion = dr["descripcion"].ToString()!,
                                Estado = dr["estado"].ToString()!,
                                Disponibilidad = dr["disponibilidad"] != DBNull.Value ? Convert.ToInt32(dr["disponibilidad"]) : 0,
                                DisponibilidadNombre = dr["disponibilidad_nombre"] != DBNull.Value ? dr["disponibilidad_nombre"].ToString() : null,
                                PropietarioNombreCompleto = !string.IsNullOrEmpty(nombreProp) ? $"{nombreProp} {apeProp}".Trim() : null,
                                TipoNombre = dr["tipo_nombre"] != DBNull.Value ? dr["tipo_nombre"].ToString() : null
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}