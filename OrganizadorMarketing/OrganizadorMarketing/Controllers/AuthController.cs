using Microsoft.AspNetCore.Mvc;
using OrganizadorMarketing.Data;
using OrganizadorMarketing.DTOs;
using OrganizadorMarketing.Auth;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace OrganizadorMarketing.Controllers
{
    // definicion del controlador
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        // inyeccion de dependencias
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;
        private readonly ILogger<AuthController> _logger;

        // inyeccion de dependencias
        public AuthController(
            ApplicationDbContext context,
            JwtService jwt,
            ILogger<AuthController> logger)
        {
            _context = context;
            _jwt = jwt;
            _logger = logger;
        }

        // configuracion del endpoint login POST /auth/login
        [HttpPost("login")]
        // consulta implementando dto
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // busqueda en base de datos
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

            // y si no existe?
            if (user == null)
            {
                // logging de intento fallido por usuario inexistente
                _logger.LogWarning("Intento de login con correo inexistente: {Correo}", dto.Correo);
                return Unauthorized("Credenciales inválidas");
            }

            // si existe verifica contraseña
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                // logging de contraseña incorrecta
                _logger.LogWarning("Contraseña incorrecta para el usuario: {Correo}", dto.Correo);
                return Unauthorized("Credenciales inválidas");
            }

            // conecta con el servicio de generacion de tokens
            var token = _jwt.GenerateToken(user);

            // logging de login exitoso
            _logger.LogInformation(
                "Login exitoso. UserId: {UserId}, Correo: {Correo}, OrganizacionId: {OrgId}",
                user.Id,
                user.Correo,
                user.OrganizacionId
            );

            // le responde OK 200 y le da el token 
            return Ok(new { token });
        }
    }
}