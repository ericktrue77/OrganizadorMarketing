using OrganizadorMarketing.Enums;

namespace OrganizadorMarketing.Models
{
    public class Usuarios
    {
        public int Id { get; set; }

        public int OrganizacionId { get; set; }
        
        //propiedad de navegacion / relacion con tabla Organizacion
        public Organizacion Organizacion { get; set; }

        public string Correo { get; set; }
        public string Password { get; set; }

        //control cerrado de roles
        public RolesUsuario Rol { get; set; }
    }
}
