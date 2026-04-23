using Microsoft.EntityFrameworkCore;
using OrganizadorMarketing.Models;
namespace OrganizadorMarketing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //enlace de modelos
        public DbSet<Organizacion> Organizaciones { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Tareas> Tareas { get; set; }





        //definicion de comportamiento del modelo dentro de l base de datos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //configurando para entidad Tareas
            modelBuilder.Entity<Tareas>()
                //relacion una tarea un usuario
                .HasOne(t => t.Asignadoa)
                //un usuario puede tener muchas tareas
                .WithMany()
                .HasForeignKey(t => t.AsignadoaId)
                //restriccion de cascadas
                .OnDelete(DeleteBehavior.Restrict); 
        }

        // Este segmento se configura para evitar múltiples rutas de cascada hacia la tabla Tareas,
        // ya que puede ser alcanzada tanto desde Organizacion como desde Usuarios.
        //
        // EF Core / SQL Server no permiten múltiples cascade paths.
        //
        // Se aplica Restrict en la relación Tareas -> Usuarios para romper una de las cascadas.
        //
        //ahora borrar una organizacion borra sus usuarios y sus tareas
        //pero no es posible borrar un usuario si aun tiene tareas

    }


}
