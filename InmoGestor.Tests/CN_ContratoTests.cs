using CapaDatos;
using CapaEntidades;
using CapaNegocio;
using Moq;
using Xunit;

namespace InmoGestor.Tests
{
    public class CN_ContratoTests
    {
        private static readonly Guid IdInmueble = Guid.NewGuid();
        private static readonly Guid IdInquilino = Guid.NewGuid();
        private static readonly Guid IdUsuario = Guid.NewGuid();
        private static readonly Guid IdRolInquilino = Guid.NewGuid();

        private readonly Mock<ICD_Contrato> _mockCdContrato;
        private readonly CN_Contrato _cnContrato;

        public CN_ContratoTests()
        {
            _mockCdContrato = new Mock<ICD_Contrato>();
            _cnContrato = new CN_Contrato(_mockCdContrato.Object, new CN_Cuota(), new CN_Inmueble());

            _mockCdContrato.Setup(x => x.ValidarInmuebleDisponible(IdInmueble, null))
                           .Returns((true, ""));
            _mockCdContrato.Setup(x => x.ObtenerIdRolInquilino(IdInquilino))
                           .Returns(IdRolInquilino);
            _mockCdContrato.Setup(x => x.CrearContrato(It.IsAny<ContratoAlquiler>()))
                           .Returns((true, "Contrato creado exitosamente", (Guid?)Guid.NewGuid()));
        }

        private ContratoAlquiler ContratoValido() => new ContratoAlquiler
        {
            IdInmueble = IdInmueble,
            IdPersonaInquilino = IdInquilino,
            CantidadCuotas = 12,
            PrecioCuota = 150000,
            FechaCreacion = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(12),
            TasaMoraMensual = 5,
            IdUsuarioCreador = IdUsuario
        };

        [Fact]
        public void CrearContrato_TodosLosCamposValidos_RetornaExito()
        {
            var (success, message, contratoId) = _cnContrato.CrearContrato(ContratoValido());

            Assert.True(success);
            Assert.Equal("Contrato creado exitosamente", message);
            Assert.NotNull(contratoId);
        }

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

        [Fact]
        public void CrearContrato_TasaMoraCero_RetornaExito()
        {
            var contrato = ContratoValido();
            contrato.TasaMoraMensual = 0;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.True(success);
            Assert.Equal("Contrato creado exitosamente", message);
            Assert.NotNull(contratoId);
        }
    }
}
