using OrganizadorMarketing.Enums;

namespace OrganizadorMarketing.Models
{
    public class Tareas
    {
        public int Id { get; set; }

        public int OrganizacionId { get; set; }

        //propiedad de navegacion / relacion con tabla Organizacion
        public Organizacion Organizacion { get; set; }

        public string Titulo { get; set; }
        public string Descripcion { get; set; }

        public int AsignadoaId { get; set; }

        //propiedad de navegacion / relacion con tabla Usuarios
        public Usuarios Asignadoa { get; set; }

        //control cerrado de estado de tarea
        public TareaEstado Estado { get; set; }

        //control cerrado de prioridad de tarea
        public TareaPrioridad Prioridad { get; set; }

        public DateTime CreacionTarea { get; set; }
        public DateTime? LimiteTarea { get; set; }
    }
}
