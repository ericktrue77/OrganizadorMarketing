using Microsoft.AspNetCore.Mvc;
using OrganizadorMarketing.Data;
using OrganizadorMarketing.DTOs;
using OrganizadorMarketing.Auth;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace OrganizadorMarketing.Controllers
{
    //definicion del controlador
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        //inyeccion de dependencias
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;

        //inyeccion de dependencias
        public AuthController(ApplicationDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        //configuracion del endpoint login POST /auth/login
        [HttpPost("login")]
        //consulta implementando dto
        public async Task<IActionResult> Login(LoginDto dto)
        {
            //busqueda en base de datos
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);
            //y si no existe?
            if (user == null)
                return Unauthorized("Credenciales inválidas");
            //si existe verifica contraseña
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Credenciales inválidas");
            //conecta con el servicio de generacion de tokens
            var token = _jwt.GenerateToken(user);
            //le responde OK 200 y le da el token 
            return Ok(new { token });
        }
    }
}
