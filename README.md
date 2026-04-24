Prueba Técnica – API de Gestión de Tareas (Multi-Tenant)
************************
Descripción
************************

API REST desarrollada en ASP.NET Core que permite la gestión de tareas para múltiples organizaciones (multi-tenant).
Cada organización tiene sus propios usuarios y tareas, completamente aisladas entre sí.

************************
Stack Tecnológico
.NET 8
ASP.NET Core Web API
Entity Framework Core
SQL Server
JWT Bearer Authentication
Swagger (Swashbuckle)
xUnit (tests)
Logging con ILogger
************************


************************
Cómo ejecutar el proyecto
Clonar el repositorio
Configurar la cadena de conexión en appsettings.json
Ejecutar el proyecto:
dotnet run
La aplicación:
Swagger: https://localhost:7207/swagger
API base: https://localhost:7207
Base de datos
nota: es necesario tener corriendo sql server o express
************************


************************
El proyecto ejecuta automáticamente:
Migraciones
Seed de datos iniciales
Datos incluidos:
2 organizaciones
1 Admin y 1 Member por organización
3 tareas por organización
************************


************************
Autenticación
Se utiliza JWT Bearer Token.
************************

************************
Endpoint:
POST /auth/login
Ejemplo:
{
  "correo": "admin1@alpha.com",
  "password": "123456"
}
************************

************************
Roles
Admin
Gestiona usuarios
Gestiona todas las tareas de su organización
Member
Puede ver todas las tareas
Solo puede crear/modificar sus propias tareas
************************

************************
Reglas de negocio implementadas
Multi-tenant basado en organizationId desde JWT
Aislamiento total entre organizaciones
Control de roles (Admin / Member)
Estados de tarea:
Pendiente
Processo
Completo
Solo el asignado o Admin puede cambiar el estado
Una tarea completada no puede modificarse por un Member
Validación de inputs en endpoints
************************

************************
Endpoints principales
POST /auth/login
GET /usuarios (Admin)
POST /usuarios
PUT /usuarios/{id}
DELETE /usuarios/{id}
GET /tareas
GET /tareas/all
GET /tareas/mis-tareas
GET /tareas/usuario/{id}
POST /tareas
PUT /tareas/{id}
PATCH /tareas/{id}/estado
DELETE /tareas/{id}
************************

************************
Testing
Se implementó un test unitario utilizando xUnit y EF Core InMemory:

Validación de regla de negocio:
Una tarea en estado "Completo" no debe poder modificarse
************************

************************
Logging
Se implementó logging básico con ILogger para:

Intentos de login
Creación de usuarios
Creación, actualización y eliminación de tareas
Eventos importantes del sistema
************************

************************
Decisiones de diseño
Uso de DTOs para evitar exponer entidades directamente
JWT incluye:
userId
organizationId
role
Filtro de datos siempre basado en organización desde el token
Controllers contienen la lógica por simplicidad (MVP)
************************

************************
Escalabilidad futura:
En un entorno productivo se migraría a:
Capas de servicios
Repository pattern
Separación de lógica de negocio
Mejor y mas robusto manejo de testing
************************

************************
Uso de IA
Se utilizó asistencia de IA para:

Resolución de dudas técnicas puntuales
Mejora de estructura de código
Revision de y deteccion de errores
************************

************************
Credenciales de prueba
Organización Alpha
Admin:
correo: admin1@alpha.com
password: 123456
Member:
correo: member1@alpha.com
password: 123456
Organización Beta
Admin:
correo: admin2@beta.com
password: 123456
Member:
correo: member2@beta.com
password: 123456
************************

************************
Notas finales
Proyecto enfocado como MVP funcional
Se priorizó claridad, seguridad y cumplimiento de requisitos
Código preparado para ser escalado en futuras iteraciones
************************
