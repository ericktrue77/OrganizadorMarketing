using Microsoft.IdentityModel.Tokens;
using OrganizadorMarketing.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrganizadorMarketing.Auth
{
    //generador de tokens
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(Usuarios user)
        {
            //informacion dentro del token
            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("organizationId", user.OrganizacionId.ToString()),
                new Claim(ClaimTypes.Role, user.Rol.ToString()),
                new Claim(ClaimTypes.Email, user.Correo)
            };
            //clave secreta del appconfig 
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );
            //firma del token
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //creacion del token con todos sus atributos (quien lo emite, para quien es, cuando expira y firma)
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    int.Parse(_config["Jwt:ExpireMinutes"])
                ),
                signingCredentials: creds
            );

            //aqui se genera el token y se convierte a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
