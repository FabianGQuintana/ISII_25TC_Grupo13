using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CapaNegocio;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CN_Usuario _cnUsuario;
        private readonly IConfiguration _configuration;
        
        public AuthController(IConfiguration configuration)
        {
            _cnUsuario = new CN_Usuario();
            _configuration = configuration;
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            try
            {
                var roles = _cnUsuario.ListarRoles();
                return Ok(new { success = true, data = roles });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, mensaje = ex.Message });
            }
        }
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var (success, usuario, message) = _cnUsuario.Login(request.Dni, request.Password);
            
            if (!success || usuario == null)
            {
                return Unauthorized(new { success = false, mensaje = message });
            }
            
            var jwtKey = _configuration["Jwt:Key"] ?? "ThisIsA32CharacterLongSecretKey!!";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "InmoGestor";
            var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
            
             var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.OPersona?.NombreCompleto ?? ""),
                new Claim("dni", usuario.OPersona?.Dni ?? ""),
                new Claim(ClaimTypes.Email, usuario.OPersona?.Email ?? ""),
                new Claim(ClaimTypes.Role, usuario.ORolUsuario?.Nombre ?? "")
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtIssuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );
            
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            return Ok(new 
            { 
                success = true, 
                token = tokenString, 
                mensaje = "Login exitoso",
                usuario = new 
                { 
                    usuarioId = usuario.IdUsuario,
                    dni = usuario.OPersona?.Dni?.ToString(),
                    nombre = usuario.OPersona?.Nombre,
                    apellido = usuario.OPersona?.Apellido,
                    email = usuario.OPersona?.Email,
                    rolNombre = usuario.ORolUsuario?.Nombre
                }
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Dni))
            {
                return BadRequest(new { success = false, mensaje = "El DNI es requerido" });
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, mensaje = "La contraseña es requerida" });
            }
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest(new { success = false, mensaje = "El nombre es requerido" });
            }
            if (string.IsNullOrWhiteSpace(request.Apellido))
            {
                return BadRequest(new { success = false, mensaje = "El apellido es requerido" });
            }
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { success = false, mensaje = "El email es requerido" });
            }
            if (string.IsNullOrWhiteSpace(request.RolNombre))
            {
                return BadRequest(new { success = false, mensaje = "El rol es requerido" });
            }

            var (success, message) = _cnUsuario.Registrar(
                request.Dni,
                request.Password,
                request.Nombre,
                request.Apellido,
                request.Email,
                request.RolNombre
            );

            if (!success)
            {
                return BadRequest(new { success = false, mensaje = message });
            }

            return Ok(new { success = true, mensaje = message });
        }
        
        /// <summary>
        /// Crea el primer usuario Superior si la base de datos está vacía.
        /// Solo funciona una vez. Una vez que existe cualquier usuario, devuelve error.
        /// Credenciales: DNI=00000000 / Password=Admin123!
        /// </summary>
        [HttpPost("seed")]
        public IActionResult Seed()
        {
            var roles = _cnUsuario.ListarRoles();
            if (roles.Count == 0)
                return Ok(new { success = false, mensaje = "No hay roles en la base de datos. Ejecutá el script SQL primero." });

            // Intentar loguear con el DNI seed — si tiene éxito, ya existe el usuario
            var (loginOk, _, _) = _cnUsuario.Login("00000000", "Admin123!");
            if (loginOk)
                return Ok(new { success = false, mensaje = "El usuario seed ya existe. Usá DNI: 00000000 / Pass: Admin123!" });

            var (success, message) = _cnUsuario.Registrar(
                dni: "00000000",
                password: "Admin123!",
                nombre: "Admin",
                apellido: "Sistema",
                email: "admin@inmogestor.com",
                rolNombre: "Superior"
            );

            return Ok(new { success, mensaje = success ? "Usuario seed creado. DNI: 00000000 / Pass: Admin123!" : message });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { success = false, mensaje = "Token inválido" });
            }
            
            var usuario = _cnUsuario.ObtenerPorId(id);
            
            if (usuario == null)
            {
                return NotFound(new { success = false, mensaje = "Usuario no encontrado" });
            }
            
            return Ok(new 
            { 
                success = true, 
                data = new 
                { 
                    usuarioId = usuario.IdUsuario,
                    nombre = usuario.OPersona?.Nombre,
                    apellido = usuario.OPersona?.Apellido,
                    email = usuario.OPersona?.Email,
                    rolNombre = usuario.ORolUsuario?.Nombre
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { success = true, mensaje = "Sesión cerrada correctamente" });
        }
    }
    
    public class LoginRequest
    {
        public string Dni { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Dni { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RolNombre { get; set; } = string.Empty;
    }
}