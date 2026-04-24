// expone solo lo necesario para ver usuarios
namespace OrganizadorMarketing.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
    }
}
