using Xunit;
using CapaNegocio;
using CapaEntidades;
using CapaDatos;
using Microsoft.Data.SqlClient;

namespace InmoGestor.Tests
{
    public class CN_ContratoTests : IDisposable
    {
        private readonly CN_Contrato _cnContrato = new();
        private readonly List<(Guid contratoId, Guid inmuebleId)> _contratosCreados = new();

        // IDs reales de la DB de desarrollo
        private static readonly Guid InmuebleDisponible1 = Guid.Parse("C22AF265-68AF-4C7A-947B-245281220A69");
        private static readonly Guid InmuebleDisponible2 = Guid.Parse("09BED20E-DB49-4865-A242-3C8A19832AF8");
        private static readonly Guid InquilinoValido = Guid.Parse("254FEC1A-A8FF-44DB-B850-1986DABCCCCC");
        private static readonly Guid UsuarioCreador = Guid.Parse("D1514450-2493-4477-A691-6A5138D84100");

        private ContratoAlquiler ContratoValido(Guid idInmueble) => new ContratoAlquiler
        {
            IdInmueble = idInmueble,
            IdPersonaInquilino = InquilinoValido,
            CantidadCuotas = 12,
            PrecioCuota = 150000,
            FechaCreacion = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(12),
            TasaMoraMensual = 5,
            IdUsuarioCreador = UsuarioCreador
        };

        [Fact]
        public void CrearContrato_TodosLosCamposValidos_RetornaExito()
        {
            var contrato = ContratoValido(InmuebleDisponible1);

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            if (success && contratoId.HasValue)
                _contratosCreados.Add((contratoId.Value, InmuebleDisponible1));

            Assert.True(success);
            Assert.Equal("Contrato creado exitosamente", message);
            Assert.NotNull(contratoId);
        }

        [Fact]
        public void CrearContrato_TasaMoraCero_RetornaExito()
        {
            var contrato = ContratoValido(InmuebleDisponible2);
            contrato.TasaMoraMensual = 0;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            if (success && contratoId.HasValue)
                _contratosCreados.Add((contratoId.Value, InmuebleDisponible2));

            Assert.True(success);
            Assert.Equal("Contrato creado exitosamente", message);
            Assert.NotNull(contratoId);
        }

        [Fact]
        public void CrearContrato_InmuebleVacio_RetornaError()
        {
            var contrato = ContratoValido(InmuebleDisponible1);
            contrato.IdInmueble = Guid.Empty;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("El inmueble es requerido", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_InquilinoVacio_RetornaError()
        {
            var contrato = ContratoValido(InmuebleDisponible1);
            contrato.IdPersonaInquilino = Guid.Empty;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("El inquilino es requerido", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_CantidadCuotasCero_RetornaError()
        {
            var contrato = ContratoValido(InmuebleDisponible1);
            contrato.CantidadCuotas = 0;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La cantidad de cuotas debe ser mayor a 0", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_CantidadCuotasNegativa_RetornaError()
        {
            var contrato = ContratoValido(InmuebleDisponible1);
            contrato.CantidadCuotas = -1;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La cantidad de cuotas debe ser mayor a 0", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_PrecioCuotaCero_RetornaError()
        {
            var contrato = ContratoValido(InmuebleDisponible1);
            contrato.PrecioCuota = 0;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("El precio de la cuota debe ser mayor a 0", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_PrecioCuotaNegativo_RetornaError()
        {
            var contrato = ContratoValido(InmuebleDisponible1);
            contrato.PrecioCuota = -500;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("El precio de la cuota debe ser mayor a 0", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_FechaFinVacia_RetornaError()
        {
            var contrato = ContratoValido(InmuebleDisponible1);
            contrato.FechaFin = default;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La fecha de fin es requerida", message);
            Assert.Null(contratoId);
        }

        public void Dispose()
        {
            if (_contratosCreados.Count == 0) return;

            using var cn = new SqlConnection(Conexion.Cadena);
            cn.Open();
            foreach (var (contratoId, inmuebleId) in _contratosCreados)
            {
                using (var cmd = new SqlCommand("DELETE FROM cuota WHERE id_contrato_alquiler = @id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", contratoId);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand("DELETE FROM contrato_alquiler WHERE id_contrato_alquiler = @id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", contratoId);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand("UPDATE inmueble SET disponibilidad = 1 WHERE id_inmueble = @id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", inmuebleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
