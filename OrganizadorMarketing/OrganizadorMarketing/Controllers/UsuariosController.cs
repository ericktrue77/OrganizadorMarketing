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

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
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
            var orgId = int.Parse(User.FindFirst("organizationId").Value);//usuario se crea dentro de su organizacion automaticamente

            //mapeo manual para el dto,, podria usarse automapper o hacer un mapeador manual segun el proyecto y los atributos
            var user = new Usuarios
            {
                Correo = dto.Correo,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),//password se hashea
                Rol = Enum.Parse<RolesUsuario>(dto.Rol),//rol viaja como string hacia el enum primero y despues se convierte
                OrganizacionId = orgId //organizacion obtenida del token
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario creado", user.Id });//respuesta Ok 200
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
                .FirstOrDefaultAsync(u => u.Id == id && u.OrganizacionId == orgId); //seguro por organizacion

            if (user == null)
                return NotFound();

            user.Correo = dto.Correo;//solo permite editar correo y rol
            user.Rol = Enum.Parse<RolesUsuario>(dto.Rol);

            await _context.SaveChangesAsync();

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
                .FirstOrDefaultAsync(u => u.Id == id && u.OrganizacionId == orgId);//no permite borrar users de otra organizacion

            if (user == null)
                return NotFound();

            _context.Usuarios.Remove(user);//eliminacion
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario eliminado" });
        }
    }
}
