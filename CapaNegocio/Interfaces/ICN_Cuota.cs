using System;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public interface ICN_Cuota
    {
        Task<CuotaCalculadaDto?> ObtenerCuotaCalculada(Guid idContrato);
    }
}
