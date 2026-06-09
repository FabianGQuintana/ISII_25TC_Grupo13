using System.Threading.Tasks;
using CapaDatos;
using CapaEntidades;
using CapaNegocio;
using Moq;
using Xunit;

namespace InmoGestor.Tests
{
    public class CN_PagoTests
    {
        private static readonly Guid IdCuotaValida = Guid.NewGuid();
        private static readonly Guid IdMetodoPagoValido = Guid.NewGuid();
        private static readonly Guid IdUsuarioValido = Guid.NewGuid();
        private static readonly Guid IdContratoValido = Guid.NewGuid();

        private readonly Mock<ICD_Pago> _mockCdPago;
        private readonly Mock<ICD_Cuota> _mockCdCuota;
        private readonly Mock<ICN_Cuota> _mockCnCuota;
        private readonly CN_Pago _cnPago;

        public CN_PagoTests()
        {
            _mockCdPago = new Mock<ICD_Pago>();
            _mockCdCuota = new Mock<ICD_Cuota>();
            _mockCnCuota = new Mock<ICN_Cuota>();
            _cnPago = new CN_Pago(_mockCdPago.Object, _mockCdCuota.Object, _mockCnCuota.Object);
        }

        [Fact]
        public async Task RegistrarPago_CuotaPendienteSinAdicionalesNiDescuento_RetornaExito()
        {
            var cuota = new Cuota
            {
                IdCuota = IdCuotaValida,
                IdContratoAlquiler = IdContratoValido,
                Estado = "Pendiente",
                NroCuota = 1,
                Periodo = "202606",
                ImporteTotalCalculado = 1000m
            };

            var cuotaCalculada = new CuotaCalculadaDto
            {
                IdCuota = IdCuotaValida.ToString(),
                NroCuota = 1,
                Periodo = "202606",
                PrecioCuota = 1000m,
                ValorIndiceAplicado = 1m,
                ImporteActualizado = 1000m,
                TotalAdicionales = 0m,
                TotalDescuentos = 0m,
                DiasAtraso = 0,
                MoraCalculada = 0m,
                TotalFinal = 1000m,
                Estado = "Pendiente"
            };

            _mockCdCuota.Setup(x => x.ObtenerPorId(IdCuotaValida)).Returns(cuota);
            _mockCnCuota.Setup(x => x.ObtenerCuotaCalculada(IdContratoValido))
                        .ReturnsAsync(cuotaCalculada);

            var (success, message, pagoId) = await _cnPago.RegistrarPago(
                IdCuotaValida, IdMetodoPagoValido, IdUsuarioValido);

            Assert.True(success);
            Assert.Equal("Pago registrado correctamente", message);
            Assert.NotEqual(Guid.Empty, pagoId);
            _mockCdPago.Verify(x => x.Insertar(It.Is<Pago>(p =>
                p.MontoTotal == 1000m
                && p.IdCuota == IdCuotaValida
                && p.IdMetodoPago == IdMetodoPagoValido
                && p.IdUsuarioCreador == IdUsuarioValido
                && p.Periodo == "202606"
                && p.MoraCobrada == 0m
                && p.Estado == "Aprobado")), Times.Once);
            _mockCdCuota.Verify(x => x.MarcarComoPagada(IdCuotaValida), Times.Once);
        }

        [Fact]
        public async Task RegistrarPago_CuotaVencidaConAdicionalesYDescuento_RetornaExito()
        {
            var cuota = new Cuota
            {
                IdCuota = IdCuotaValida,
                IdContratoAlquiler = IdContratoValido,
                Estado = "Vencida",
                NroCuota = 3,
                Periodo = "202606",
                ImporteTotalCalculado = 1000m,
                DescuentoAdicionalTotal = 200m,
                OtrosAdicionalesTotal = 500m
            };

            var cuotaCalculada = new CuotaCalculadaDto
            {
                IdCuota = IdCuotaValida.ToString(),
                NroCuota = 3,
                Periodo = "202606",
                PrecioCuota = 1000m,
                ValorIndiceAplicado = 1m,
                ImporteActualizado = 1000m,
                TotalAdicionales = 500m,
                TotalDescuentos = 200m,
                DiasAtraso = 5,
                MoraCalculada = 50m,
                TotalFinal = 1350m,
                Estado = "Vencida"
            };

            _mockCdCuota.Setup(x => x.ObtenerPorId(IdCuotaValida)).Returns(cuota);
            _mockCnCuota.Setup(x => x.ObtenerCuotaCalculada(IdContratoValido))
                        .ReturnsAsync(cuotaCalculada);

            var (success, message, pagoId) = await _cnPago.RegistrarPago(
                IdCuotaValida, IdMetodoPagoValido, IdUsuarioValido);

            Assert.True(success);
            Assert.Equal("Pago registrado correctamente", message);
            Assert.NotEqual(Guid.Empty, pagoId);
            _mockCdPago.Verify(x => x.Insertar(It.Is<Pago>(p =>
                p.MontoTotal == 1350m
                && p.MoraCobrada == 50m
                && p.Periodo == "202606")), Times.Once);
            _mockCdCuota.Verify(x => x.MarcarComoPagada(IdCuotaValida), Times.Once);
        }

        [Fact]
        public async Task RegistrarPago_CuotaInvalidaOVacia_RetornaError()
        {
            _mockCdCuota.Setup(x => x.ObtenerPorId(It.IsAny<Guid>()))
                        .Returns((Cuota?)null);

            var (success, message, pagoId) = await _cnPago.RegistrarPago(
                Guid.Empty, IdMetodoPagoValido, IdUsuarioValido);

            Assert.False(success);
            Assert.Equal("No se encontró la cuota", message);
            Assert.Equal(Guid.Empty, pagoId);
            _mockCdPago.Verify(x => x.Insertar(It.IsAny<Pago>()), Times.Never);
            _mockCdCuota.Verify(x => x.MarcarComoPagada(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RegistrarPago_MetodoPagoVacio_RetornaError()
        {
            var cuota = new Cuota
            {
                IdCuota = IdCuotaValida,
                IdContratoAlquiler = IdContratoValido,
                Estado = "Pendiente"
            };

            var cuotaCalculada = new CuotaCalculadaDto
            {
                IdCuota = IdCuotaValida.ToString(),
                Periodo = "202606",
                PrecioCuota = 1000m,
                ImporteActualizado = 1000m,
                TotalFinal = 1000m,
                Estado = "Pendiente"
            };

            _mockCdCuota.Setup(x => x.ObtenerPorId(IdCuotaValida)).Returns(cuota);
            _mockCnCuota.Setup(x => x.ObtenerCuotaCalculada(IdContratoValido))
                        .ReturnsAsync(cuotaCalculada);

            var (success, message, pagoId) = await _cnPago.RegistrarPago(
                IdCuotaValida, Guid.Empty, IdUsuarioValido);

            Assert.False(success);
            Assert.Equal("El método de pago es requerido", message);
            Assert.Equal(Guid.Empty, pagoId);
            _mockCdPago.Verify(x => x.Insertar(It.IsAny<Pago>()), Times.Never);
            _mockCdCuota.Verify(x => x.MarcarComoPagada(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RegistrarPago_CuotaYaPagada_RetornaError()
        {
            var cuota = new Cuota
            {
                IdCuota = IdCuotaValida,
                IdContratoAlquiler = IdContratoValido,
                Estado = "Pagada"
            };

            _mockCdCuota.Setup(x => x.ObtenerPorId(IdCuotaValida)).Returns(cuota);

            var (success, message, pagoId) = await _cnPago.RegistrarPago(
                IdCuotaValida, IdMetodoPagoValido, IdUsuarioValido);

            Assert.False(success);
            Assert.Equal("La cuota no está pendiente o vencida", message);
            Assert.Equal(Guid.Empty, pagoId);
            _mockCdPago.Verify(x => x.Insertar(It.IsAny<Pago>()), Times.Never);
            _mockCdCuota.Verify(x => x.MarcarComoPagada(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RegistrarPago_MontoDescuentoMayorAlTotal_RetornaError()
        {
            var cuota = new Cuota
            {
                IdCuota = IdCuotaValida,
                IdContratoAlquiler = IdContratoValido,
                Estado = "Pendiente"
            };

            var cuotaCalculada = new CuotaCalculadaDto
            {
                IdCuota = IdCuotaValida.ToString(),
                Periodo = "202606",
                PrecioCuota = 500m,
                ImporteActualizado = 500m,
                TotalAdicionales = 0m,
                TotalDescuentos = 600m,
                DiasAtraso = 0,
                MoraCalculada = 0m,
                TotalFinal = -100m,
                Estado = "Pendiente"
            };

            _mockCdCuota.Setup(x => x.ObtenerPorId(IdCuotaValida)).Returns(cuota);
            _mockCnCuota.Setup(x => x.ObtenerCuotaCalculada(IdContratoValido))
                        .ReturnsAsync(cuotaCalculada);

            var (success, message, pagoId) = await _cnPago.RegistrarPago(
                IdCuotaValida, IdMetodoPagoValido, IdUsuarioValido);

            Assert.False(success);
            Assert.Equal("El descuento no puede ser mayor al total de la cuota", message);
            Assert.Equal(Guid.Empty, pagoId);
            _mockCdPago.Verify(x => x.Insertar(It.IsAny<Pago>()), Times.Never);
            _mockCdCuota.Verify(x => x.MarcarComoPagada(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RegistrarPago_CuotaAnulada_RetornaError()
        {
            var cuota = new Cuota
            {
                IdCuota = IdCuotaValida,
                IdContratoAlquiler = IdContratoValido,
                Estado = "Anulada"
            };

            _mockCdCuota.Setup(x => x.ObtenerPorId(IdCuotaValida)).Returns(cuota);

            var (success, message, pagoId) = await _cnPago.RegistrarPago(
                IdCuotaValida, IdMetodoPagoValido, IdUsuarioValido);

            Assert.False(success);
            Assert.Equal("La cuota no está pendiente o vencida", message);
            Assert.Equal(Guid.Empty, pagoId);
            _mockCdPago.Verify(x => x.Insertar(It.IsAny<Pago>()), Times.Never);
            _mockCdCuota.Verify(x => x.MarcarComoPagada(It.IsAny<Guid>()), Times.Never);
        }
    }
}
