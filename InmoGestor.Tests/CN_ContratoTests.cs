using Xunit;
using CapaNegocio;
using CapaEntidades;

namespace InmoGestor.Tests
{
    public class CN_ContratoTests
    {
        private readonly CN_Contrato _cnContrato = new();

        private ContratoAlquiler ContratoValido() => new ContratoAlquiler
        {
            IdInmueble = Guid.NewGuid(),
            IdPersonaInquilino = Guid.NewGuid(),
            CantidadCuotas = 12,
            PrecioCuota = 150000,
            FechaFin = DateTime.Now.AddMonths(12),
            TasaMoraMensual = 5,
            IdUsuarioCreador = Guid.NewGuid()
        };

        [Fact]
        public void CrearContrato_InmuebleVacio_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.IdInmueble = Guid.Empty;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("El inmueble es requerido", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_InquilinoVacio_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.IdPersonaInquilino = Guid.Empty;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("El inquilino es requerido", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_CantidadCuotasCero_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.CantidadCuotas = 0;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La cantidad de cuotas debe ser mayor a 0", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_CantidadCuotasNegativa_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.CantidadCuotas = -1;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La cantidad de cuotas debe ser mayor a 0", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_PrecioCuotaCero_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.PrecioCuota = 0;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("El precio de la cuota debe ser mayor a 0", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_PrecioCuotaNegativo_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.PrecioCuota = -500;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("El precio de la cuota debe ser mayor a 0", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_FechaFinVacia_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.FechaFin = default;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La fecha de fin es requerida", message);
            Assert.Null(contratoId);
        }
    }
}