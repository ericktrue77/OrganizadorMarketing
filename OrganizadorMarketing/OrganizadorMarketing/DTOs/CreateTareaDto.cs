namespace OrganizadorMarketing.DTOs
{
    public class CreateTareaDto
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int AsignadoaId { get; set; }
        public string Prioridad { get; set; }
        public DateTime? LimiteTarea { get; set; }
    }
}
