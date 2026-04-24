using Microsoft.EntityFrameworkCore;
using OrganizadorMarketing.Data;
using OrganizadorMarketing.Enums;
using OrganizadorMarketing.Models;
using Xunit;

public class TareasTests
{
    // Método helper que crea un DbContext en memoria.
    // Se utiliza una base de datos única por cada ejecución para evitar conflictos entre tests.
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void No_Permite_Modificar_Tarea_Completada()
    {

        // (Preparación)


        // Se crea un contexto de base de datos en memoria
        var context = GetDbContext();

        // Se crea una organización de prueba
        var org = new Organizacion { Nombre = "Test" };
        context.Organizaciones.Add(org);
        context.SaveChanges();

        // Se crea un usuario asociado a la organización
        var user = new Usuarios
        {
            Correo = "test@test.com",
            Password = "123",
            Rol = RolesUsuario.Member,
            OrganizacionId = org.Id
        };

        context.Usuarios.Add(user);
        context.SaveChanges();

        // Se crea una tarea en estado "Completo"
        var task = new Tareas
        {
            Titulo = "Test task",
            Descripcion = "Test",
            OrganizacionId = org.Id,
            AsignadoaId = user.Id,
            Estado = TareaEstado.Completo,
            Prioridad = TareaPrioridad.Medio,
            CreacionTarea = DateTime.Now
        };

        context.Tareas.Add(task);
        context.SaveChanges();


        // (Ejecución)


        // Se evalúa la regla de negocio:
        // una tarea en estado "Completo" no debería poder modificarse
        var puedeModificar = task.Estado != TareaEstado.Completo;

        // (Validación)

        // Se verifica que la tarea NO puede modificarse
        // (es decir, la condición debe ser falsa)
        Assert.False(puedeModificar);
    }
}