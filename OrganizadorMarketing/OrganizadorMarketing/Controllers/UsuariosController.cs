//controlador a 3 capas
//DTOs
//JWT
//EF Core
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizadorMarketing.Data;
using OrganizadorMarketing.DTOs;
using OrganizadorMarketing.Enums;
using OrganizadorMarketing.Models;

namespace OrganizadorMarketing.Controllers
{
    [ApiController]
    [Route("usuarios")]//ruta
    [Authorize(Roles = "Admin")]//decorador para rol aplicado a toda la ruta y endpoint por endpoint
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(ApplicationDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET all por organizacion
        //flujo
        //Llega request con token
        //token da... userId, orgId, role
        //Controller lee orgId
        //Consulta solo esa org
        //Devuelve DTOs
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);//uso del token

            var usuarios = await _context.Usuarios
                .Where(u => u.OrganizacionId == orgId)//query Cada admin solo ve su organización
                .Select(u => new UsuarioDto//no devuelve entidad solo DTO
                {
                    Id = u.Id,
                    Correo = u.Correo,
                    Rol = u.Rol.ToString()
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        //POST crear usuario
        // flujo
        // Llega request con token + datos (CreateUserDto)
        // Token da: userId, orgId, role
        // [Authorize] valida que sea Admin
        // Controller lee orgId desde el token
        // Recibe dto (correo, password, rol)
        // Convierte dto → modelo Usuarios
        // Hashea password con BCrypt
        // Convierte rol string → enum
        // Asigna OrganizacionId desde el token (NO desde el cliente)
        // Guarda en base de datos
        // Devuelve respuesta simple (id del usuario creado)
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);

            // validar duplicado
            var exists = await _context.Usuarios
                .AnyAsync(u => u.Correo == dto.Correo && u.OrganizacionId == orgId);

            if (exists)
                return BadRequest("El correo ya está registrado en esta organización");

            // validar rol
            if (!Enum.TryParse<RolesUsuario>(dto.Rol, true, out var rolEnum))
                return BadRequest("Rol inválido");

            var user = new Usuarios
            {
                Correo = dto.Correo,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = rolEnum,
                OrganizacionId = orgId
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario creado: {UserId} en organización {OrgId}", user.Id, orgId);
            return Ok(new { message = "Usuario creado", user.Id });
        }

        //PUT usuarios 
        // flujo
        // Llega request con token + datos (UpdateUserDto)
        // Token da: userId, orgId, role
        // [Authorize] valida que sea Admin
        // Controller lee orgId desde el token
        // Busca usuario por Id Y OrganizacionId
        // Si no existe → NotFound
        // Si existe:
        //   - Actualiza correo
        //   - Convierte rol string → enum
        // Guarda cambios en base de datos
        // Devuelve mensaje de éxito
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("Usuario no existe");

            if (user.OrganizacionId != orgId)
                return BadRequest("El usuario no pertenece a tu organización");

            // validar duplicado
            var exists = await _context.Usuarios
                .AnyAsync(u => u.Correo == dto.Correo
                            && u.OrganizacionId == orgId
                            && u.Id != id);

            if (exists)
                return BadRequest("El correo ya está en uso en esta organización");

            // validar rol
            if (!Enum.TryParse<RolesUsuario>(dto.Rol, true, out var rolEnum))
                return BadRequest("Rol inválido");

            user.Correo = dto.Correo;
            user.Rol = rolEnum;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario actualizado: {UserId}", user.Id);
            return Ok(new { message = "Usuario actualizado" });
        }

        //DELETE usuarios
        // flujo
        // Llega request con token + id del usuario
        // Token da: userId, orgId, role
        // [Authorize] valida que sea Admin
        // Controller lee orgId desde el token
        // Busca usuario por Id Y OrganizacionId
        // Si no existe → NotFound
        // Si existe:
        //   - Elimina usuario
        // Guarda cambios en base de datos
        // Devuelve mensaje de éxito
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("Usuario no existe");

            if (user.OrganizacionId != orgId)
                return BadRequest("El usuario no pertenece a tu organización");

            _context.Usuarios.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario eliminado: {UserId}", user.Id);
            return Ok(new { message = "Usuario eliminado" });
        }
    }
}
