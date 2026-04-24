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
using System.Security.Claims;

namespace OrganizadorMarketing.Controllers
{
    [ApiController]
    [Route("tareas")]//ruta
    [Authorize]//seguro de autenticacion minimo para todo el controller
    public class TareasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TareasController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("mis-tareas")]
        [Authorize]
        public async Task<IActionResult> GetMisTareas()
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);
            var userId = int.Parse(User.FindFirst("userId").Value);

            var tareas = await _context.Tareas
                .Include(t => t.Asignadoa)
                .Where(t => t.OrganizacionId == orgId && t.AsignadoaId == userId)
                .Select(t => new TareaDto
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Estado = t.Estado.ToString(),
                    Prioridad = t.Prioridad.ToString(),
                    LimiteTarea = t.LimiteTarea,
                    Asignado = t.Asignadoa.Correo
                })
                .ToListAsync();

            return Ok(tareas);
        }


        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllTareas()
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);

            var tareas = await _context.Tareas
                .Include(t => t.Asignadoa)
                .Where(t => t.OrganizacionId == orgId)
                .Select(t => new TareaDto
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Estado = t.Estado.ToString(),
                    Prioridad = t.Prioridad.ToString(),
                    LimiteTarea = t.LimiteTarea,
                    Asignado = t.Asignadoa.Correo
                })
                .ToListAsync();

            return Ok(tareas);
        }



        [HttpGet("usuario/{Id}")]
        [Authorize]
        public async Task<IActionResult> GetTareasPorUsuario(int Id)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);

            // validar que el usuario pertenece a la organización
            var userExists = await _context.Usuarios
                .AnyAsync(u => u.Id == Id && u.OrganizacionId == orgId);

            if (!userExists)
                return NotFound("El usuario no pertenece a tu organización");

            var tareas = await _context.Tareas
                .Include(t => t.Asignadoa)
                .Where(t => t.OrganizacionId == orgId && t.AsignadoaId == Id)
                .Select(t => new TareaDto
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Estado = t.Estado.ToString(),
                    Prioridad = t.Prioridad.ToString(),
                    LimiteTarea = t.LimiteTarea,
                    Asignado = t.Asignadoa.Correo
                })
                .ToListAsync();

            return Ok(tareas);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetTareas([FromQuery] string status, [FromQuery] int? asignadoId)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);

            var query = _context.Tareas
                .Include(t => t.Asignadoa)
                .Where(t => t.OrganizacionId == orgId);

            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse<TareaEstado>(status, true, out var estadoEnum))
                    return BadRequest("Estado inválido");

                query = query.Where(t => t.Estado == estadoEnum);
            }

            if (asignadoId.HasValue)
            {
                query = query.Where(t => t.AsignadoaId == asignadoId);
            }

            var result = await query
                .Select(t => new TareaDto
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descripcion = t.Descripcion,
                    Estado = t.Estado.ToString(),
                    Prioridad = t.Prioridad.ToString(),
                    LimiteTarea = t.LimiteTarea,
                    Asignado = t.Asignadoa.Correo
                })
                .ToListAsync();

            return Ok(result);
        }

        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTask(CreateTareaDto dto)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);
            var userId = int.Parse(User.FindFirst("userId").Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            int asignadoId;

            if (role == "Member")
            {
                // VALIDAR intento de asignación indebida
                if (dto.AsignadoaId != userId)
                    return BadRequest("No tienes permiso para crear tareas a otros usuarios");

                asignadoId = userId;
            }
            else if (role == "Admin")
            {
                var userAsignado = await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.Id == dto.AsignadoaId &&
                        u.OrganizacionId == orgId);

                if (userAsignado == null)
                    return BadRequest("El usuario no pertenece a tu organización");

                asignadoId = dto.AsignadoaId;
            }
            else
            {
                return Forbid("No tienes permiso de ejecutar esta acción");
            }

            var task = new Tareas
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                AsignadoaId = asignadoId,
                OrganizacionId = orgId,
                Estado = TareaEstado.Pendiente,
                Prioridad = Enum.Parse<TareaPrioridad>(dto.Prioridad, true),
                CreacionTarea = DateTime.Now,
                LimiteTarea = dto.LimiteTarea
            };

            _context.Tareas.Add(task);
            await _context.SaveChangesAsync();

            await _context.Entry(task)
                .Reference(t => t.Asignadoa)
                .LoadAsync();

            var response = new TareaDetalleDto
            {
                Id = task.Id,
                Titulo = task.Titulo,
                Descripcion = task.Descripcion,
                Estado = task.Estado.ToString(),
                Prioridad = task.Prioridad.ToString(),
                LimiteTarea = task.LimiteTarea,
                Asignado = task.Asignadoa.Correo
            };

            return Ok(response);
        }

        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTareaDto dto)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);

            // 1. buscar tarea sin filtrar org primero
            var task = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == id);

            // 2. si no existe
            if (task == null)
                return NotFound("La tarea no existe");

            // 3. validar organización (AQUÍ está el cambio importante)
            if (task.OrganizacionId != orgId)
                return BadRequest("No puedes editar tareas de otras organizaciones");

            //  VALIDAR usuario pertenece a la misma org
            var userAsignado = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == dto.AsignadoaId && u.OrganizacionId == orgId);

            if (userAsignado == null)
                return BadRequest("El usuario no pertenece a tu organización");

            task.Titulo = dto.Titulo;
            task.Descripcion = dto.Descripcion;
            task.AsignadoaId = dto.AsignadoaId;

            if (!Enum.TryParse<TareaPrioridad>(dto.Prioridad, true, out var prioridad))
                return BadRequest("Prioridad inválida");

            task.Prioridad = prioridad;

            if (!string.IsNullOrEmpty(dto.Estado))
            {
                if (!Enum.TryParse<TareaEstado>(dto.Estado, true, out var estado))
                    return BadRequest("Estado inválido");

                task.Estado = estado;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Tarea actualizada" });
        }

        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);

            var task = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound("La tarea no existe");

            // VALIDACIÓN EXPLÍCITA DE ORGANIZACIÓN
            if (task.OrganizacionId != orgId)
                return BadRequest("No puedes borrar tareas de otra organización");

            _context.Tareas.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tarea eliminada" });
        }

        
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> UpdateStatus(int id, TareaStatusDto dto)
        {
            var orgId = int.Parse(User.FindFirst("organizationId").Value);
            var userId = int.Parse(User.FindFirst("userId").Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var task = await _context.Tareas
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound("La tarea no existe");

            if (task.OrganizacionId != orgId)
                return BadRequest("No puedes modificar tareas de otras organizaciones");

            if (role != "Admin" && task.Estado == TareaEstado.Completo)
                return BadRequest("No puedes modificar una tarea completada");

            if (role != "Admin" && task.AsignadoaId != userId)
                return BadRequest("No puedes modificar tareas que no te pertenecen");

            if (!Enum.TryParse<TareaEstado>(dto.Estado, true, out var nuevoEstado))
                return BadRequest("Estado inválido");

            task.Estado = nuevoEstado;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Estado actualizado" });
        }
    }
}
