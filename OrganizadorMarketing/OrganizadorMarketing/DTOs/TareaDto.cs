public class TareaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string Estado { get; set; }
    public string Prioridad { get; set; }
    public DateTime? LimiteTarea { get; set; }
    public string Asignado { get; set; }
}