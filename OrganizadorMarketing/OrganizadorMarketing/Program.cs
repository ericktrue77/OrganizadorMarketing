

//registro y enlace de jwt
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrganizadorMarketing.Auth;


using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using OrganizadorMarketing.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//bloque de configuracion de swagger para usar JWT
builder.Services.AddSwaggerGen(options =>
{
    //definimos el esquema de seguridad brearer con sus propiedades
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization", //header
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,//autenticación HTTP estándar
        Scheme = "bearer",//se manda como Bearer Token
        BearerFormat = "JWT",//El token es JWT
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,//El token va en los headers, no en query ni body
        Description = "Ingrese el token JWT así: Bearer {token}"//texto que se ve en swagger
    });
    //Todas las operaciones requieren este esquema de seguridad
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            //Usa el esquema de seguridad llamado Bearer definido antes
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            //Aplica esto globalmente a todos los endpoints
            new string[] {}
        }
    });
});


//Registro del servicio de JWT,, scoped/una instancia por request
builder.Services.AddScoped<JwtService>();

//configura autenticacion
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
        //valida token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });
//activamos autorizacion/[Authorize]
builder.Services.AddAuthorization();


var app = builder.Build();

//enlace seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    context.Database.Migrate();//migracion automatica
    SeedData.Initialize(context);//ejecucion de llenado a BD
}




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//orden importante,, primero lee valida y despues autoriza
app.UseAuthentication();
//agregando el auth para jwt
app.UseAuthorization();

app.MapControllers();

app.Run();
