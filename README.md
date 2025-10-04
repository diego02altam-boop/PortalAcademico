🏫 Portal Académico — Gestión de Cursos y Matrículas
📋 Descripción del Proyecto

Aplicación web desarrollada en ASP.NET Core MVC (.NET 8) con autenticación por Identity, administración de cursos, estudiantes y matrículas.
Permite a los estudiantes inscribirse en cursos activos y a los coordinadores académicos gestionar la oferta educativa.

🧱 Stack Tecnológico

Framework: ASP.NET Core MVC (.NET 8)

ORM: Entity Framework Core (SQLite)

Autenticación: Identity

Frontend: Razor Views + Bootstrap

Sesión y Cache: Redis

Despliegue: Render.com (Web Service)

Control de versiones: Git y GitHub (flujo GitFlow)

⚙️ Estructura de Ramas y Funcionalidades
Rama	Funcionalidad	Descripción breve
feature/bootstrap-dominio	Modelo de datos	Curso y Matrícula con restricciones (créditos, cupos, horarios)
feature/catalogo-cursos	Catálogo de cursos	Filtros por nombre, créditos y horario, con botón “Inscribirse”
feature/matriculas	Inscripción	Validaciones de cupo, horarios y usuario autenticado
feature/sesion-redis	Redis	Guarda último curso visitado y cachea el listado de cursos
feature/panel-coordinador	Panel del coordinador	CRUD de cursos y gestión de matrículas
deploy/render	Despliegue	Configuración del Web Service en Render
🚀 Pasos para Ejecutar Localmente

Clonar el repositorio:

git clone https://github.com/diego02altam-boop/PortalAcademico.git
cd PortalAcademico


Restaurar dependencias:

dotnet restore


Aplicar migraciones y crear la base de datos:

dotnet ef database update


Ejecutar el proyecto:

dotnet run

🧩 Variables de Entorno

Estas variables deben configurarse tanto localmente como en Render:

Variable	Descripción
ASPNETCORE_ENVIRONMENT	Debe ser Production para Render
ASPNETCORE_URLS	http://0.0.0.0:${PORT}
ConnectionStrings__DefaultConnection	Cadena de conexión a la base de datos
Redis__ConnectionString	Cadena de conexión a Redis
☁️ Despliegue en Render

Crear un nuevo Web Service en Render.com
.

Seleccionar el repositorio PortalAcademico.

Configurar:

Language: Docker

Root Directory: PortalAcademico

Dockerfile Path: ./Dockerfile

Environment Variables: según la tabla anterior.

Hacer clic en Deploy Web Service.

🔑 Credenciales Iniciales
Usuario	Rol	Contraseña
admin@portal.com
	Coordinador	Admin123*
🌐 URL del Proyecto en Render

👉 https://portal-academico.onrender.com

🧠 Criterios de Evaluación

Ramas y merges correctamente aplicados.

Validaciones de servidor funcionales.

Redis funcionando correctamente en Render.

README completo con instrucciones, variables y URL de despliegue
