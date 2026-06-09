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

        private void SetupInmuebleDisponible() =>
            _mockCdContrato.Setup(x => x.ValidarInmuebleDisponible(IdInmueble, null))
                           .Returns((true, ""));

        private void SetupRolInquilino() =>
            _mockCdContrato.Setup(x => x.ObtenerIdRolInquilino(IdInquilino))
                           .Returns(IdRolInquilino);

        private void SetupCrearContrato(Guid idContrato) =>
            _mockCdContrato.Setup(x => x.CrearContrato(It.IsAny<ContratoAlquiler>()))
                           .Returns((true, "Contrato creado exitosamente", (Guid?)idContrato));

        [Fact]
        public void CrearContrato_TodosLosCamposValidos_RetornaExito()
        {
            var idEsperado = Guid.NewGuid();
            SetupInmuebleDisponible();
            SetupRolInquilino();
            SetupCrearContrato(idEsperado);

            var (success, message, contratoId) = _cnContrato.CrearContrato(ContratoValido());

            Assert.True(success);
            Assert.Equal("Contrato creado exitosamente", message);
            Assert.Equal(idEsperado, contratoId);
        }

        [Fact]
        public void CrearContrato_TasaMoraCero_RetornaExito()
        {
            var idEsperado = Guid.NewGuid();
            SetupInmuebleDisponible();
            SetupRolInquilino();
            SetupCrearContrato(idEsperado);
            var contrato = ContratoValido();
            contrato.TasaMoraMensual = 0;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.True(success);
            Assert.Equal("Contrato creado exitosamente", message);
            Assert.Equal(idEsperado, contratoId);
        }

        [Fact]
        public void CrearContrato_InmuebleNoDisponible_RetornaError()
        {
            _mockCdContrato.Setup(x => x.ValidarInmuebleDisponible(IdInmueble, null))
                           .Returns((false, "El inmueble ya tiene un contrato activo"));
            SetupRolInquilino();

            var (success, message, contratoId) = _cnContrato.CrearContrato(ContratoValido());

            Assert.False(success);
            Assert.Equal("El inmueble ya tiene un contrato activo", message);
            Assert.Null(contratoId);
            _mockCdContrato.Verify(x => x.CrearContrato(It.IsAny<ContratoAlquiler>()), Times.Never);
        }

        [Fact]
        public void CrearContrato_InquilinoSinRolAsignado_RetornaError()
        {
            SetupInmuebleDisponible();
            _mockCdContrato.Setup(x => x.ObtenerIdRolInquilino(IdInquilino))
                           .Returns((Guid?)null);

            var (success, message, contratoId) = _cnContrato.CrearContrato(ContratoValido());

            Assert.False(success);
            Assert.Equal("El inquilino seleccionado no tiene un rol 'Inquilino' válido asignado en el sistema.", message);
            Assert.Null(contratoId);
            _mockCdContrato.Verify(x => x.CrearContrato(It.IsAny<ContratoAlquiler>()), Times.Never);
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
        public void CrearContrato_CantidadCuotasSuperaMaximo_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.CantidadCuotas = CN_Contrato.MaxCantidadCuotas + 1;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal($"La cantidad de cuotas no puede superar {CN_Contrato.MaxCantidadCuotas}", message);
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
        public void CrearContrato_TasaMoraNegativa_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.TasaMoraMensual = -1;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La tasa de mora no puede ser negativa", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_FrecuenciaAjusteInvalida_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.FrecuenciaAjuste = "Diaria";

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La frecuencia de ajuste no es válida", message);
            Assert.Null(contratoId);
        }

        [Fact]
        public void CrearContrato_IndiceSeleccionadoSinValor_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.IdTipoIndice = Guid.NewGuid();
            contrato.ValorIndiceInicio = null;

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("Debe ingresar el valor del índice seleccionado", message);
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
        public void CrearContrato_FechaFinAnteriorAInicio_RetornaError()
        {
            var contrato = ContratoValido();
            contrato.FechaCreacion = DateTime.Now;
            contrato.FechaFin = DateTime.Now.AddDays(-1);

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            Assert.False(success);
            Assert.Equal("La fecha de fin debe ser posterior a la fecha de inicio", message);
            Assert.Null(contratoId);
        }
    }
}
