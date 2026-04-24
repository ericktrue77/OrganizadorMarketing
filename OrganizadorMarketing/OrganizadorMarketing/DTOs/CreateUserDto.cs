//recibe datos para crear usuarios
namespace OrganizadorMarketing.DTOs
{
    public class CreateUserDto
    {
        public string Correo { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; } // "Admin" o "Member"
    }
}
