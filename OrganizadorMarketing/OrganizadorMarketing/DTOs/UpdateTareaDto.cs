//dto para actualizar tareas
namespace OrganizadorMarketing.DTOs
{
    public class UpdateTareaDto
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int AsignadoaId { get; set; }
        public string Prioridad { get; set; }
        public string Estado { get; set; }
        public DateTime? LimiteTarea { get; set; }
    }
}
