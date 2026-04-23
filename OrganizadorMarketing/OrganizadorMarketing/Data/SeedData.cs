using OrganizadorMarketing.Models;
using OrganizadorMarketing.Enums;

namespace OrganizadorMarketing.Data
{
    //llenar la base de datos automáticamente con datos iniciales
    public static class SeedData
    {
        //permite Insertar datos,Guardar en SQL, Consultar si ya existen dato
        public static void Initialize(ApplicationDbContext context)
        {
            //proteccion contra duplicados y condicion de aplicacion(existiendo una organizacion no entra el seed
            //incluso si esta se crea manualmente con sql crudo y se borran las 2 existentes detectando una organizacion
            //no entra,, al igual se puede hacer mas robusto segun el proyecto y las condiciones)
            if (context.Organizaciones.Any())
                return;

            // creando organizaciones en memoria no en DB aun
            var org1 = new Organizacion { Nombre = "Agencia Alpha" };
            var org2 = new Organizacion { Nombre = "Agencia Beta" };

            //llevandolas a BD
            context.Organizaciones.AddRange(org1, org2);
            context.SaveChanges();

            // creando usuarios de organizacion 1 en mememoria no en DB aun
            var admin1 = new Usuarios
            {
                Correo = "admin1@alpha.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),//pasword hasheado
                Rol = RolesUsuario.Admin,
                OrganizacionId = org1.Id//coneccion con su organizacion
            };

            var member1 = new Usuarios
            {
                Correo = "member1@alpha.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),//pasword hasheado
                Rol = RolesUsuario.Member,
                OrganizacionId = org1.Id//coneccion con su organizacion
            };

            // // creando usuarios de organizacion 2 en mememoria no en DB aun
            var admin2 = new Usuarios
            {
                Correo = "admin2@beta.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),//pasword hasheado
                Rol = RolesUsuario.Admin,
                OrganizacionId = org2.Id//coneccion con su organizacion
            };

            var member2 = new Usuarios
            {
                Correo = "member2@beta.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),//pasword hasheado
                Rol = RolesUsuario.Member,
                OrganizacionId = org2.Id//coneccion con su organizacion
            };
            //enviando usuarios a BD
            context.Usuarios.AddRange(admin1, member1, admin2, member2);
            context.SaveChanges();

            // Creando tareas para los usuarios de la organizacion 1 en memoria no en DB aun
            var task1 = new Tareas
            {
                Titulo = "Diseñar campaña Facebook",
                Descripcion = "Campaña inicial de ads",
                OrganizacionId = org1.Id,//coneccion con su organizacion
                AsignadoaId = admin1.Id,//coneccion con su usuario
                Estado = TareaEstado.Pendiente,
                Prioridad = TareaPrioridad.Alto,
                CreacionTarea = DateTime.Now
            };

            var task2 = new Tareas
            {
                Titulo = "Crear contenido Instagram",
                Descripcion = "Posts semanales",
                OrganizacionId = org1.Id,//coneccion con su organizacion
                AsignadoaId = member1.Id,//coneccion con su usuario
                Estado = TareaEstado.Processo,
                Prioridad = TareaPrioridad.Medio,
                CreacionTarea = DateTime.Now
            };

            var task3 = new Tareas
            {
                Titulo = "Analizar métricas",
                Descripcion = "Reporte mensual",
                OrganizacionId = org1.Id,//coneccion con su organizacion
                AsignadoaId = admin1.Id,//coneccion con su usuario
                Estado = TareaEstado.Pendiente,
                Prioridad = TareaPrioridad.Bajo,
                CreacionTarea = DateTime.Now
            };

            // Creando tareas para los usuarios de la organizacion 2 en memoria no en DB aun
            var task4 = new Tareas
            {
                Titulo = "SEO página web",
                Descripcion = "Optimización inicial",
                OrganizacionId = org2.Id,//coneccion con su organizacion
                AsignadoaId = admin2.Id,//coneccion con su usuario
                Estado = TareaEstado.Pendiente,
                Prioridad = TareaPrioridad.Alto,
                CreacionTarea = DateTime.Now
            };

            var task5 = new Tareas
            {
                Titulo = "Redacción blog",
                Descripcion = "Artículos semanales",
                OrganizacionId = org2.Id,//coneccion con su organizacion
                AsignadoaId = member2.Id,//coneccion con su usuario
                Estado = TareaEstado.Processo,
                Prioridad = TareaPrioridad.Medio,
                CreacionTarea = DateTime.Now
            };

            var task6 = new Tareas
            {
                Titulo = "Optimizar ads Google",
                Descripcion = "Campañas PPC",
                OrganizacionId = org2.Id,//coneccion con su organizacion
                AsignadoaId = admin2.Id,//coneccion con su usuario
                Estado = TareaEstado.Pendiente,
                Prioridad = TareaPrioridad.Alto,
                CreacionTarea = DateTime.Now
            };

            //enviando las tareas a BD
            context.Tareas.AddRange(task1, task2, task3, task4, task5, task6);

            context.SaveChanges();
        }
    }
}
