# Bitácora del proyecto (pasos realizados)

Documento de contexto: describe **qué se hizo, en qué orden y por qué** para
construir esta Web API REST con EF Core Code First. Sirve como guía para
reproducir el proyecto o entender su estado actual.

---

## 0. Contexto y restricciones

- **Objetivo:** Web API REST paso a paso aplicando **Code First** (entidades →
  migraciones → controladores) con operaciones basadas en el principio REST.
- **Entorno de trabajo:** Windows, Visual Studio Code.
- **Restricción encontrada:** el equipo **no tiene permisos de administrador**,
  por lo que **no fue posible instalar** el SDK de .NET ni SQL Server. La
  instalación con `winget` devolvió el **código 1602** ("Ha cancelado la
  instalación"), típico cuando se rechaza el UAC o la cuenta no es administrador.
- **Decisión:** se escribió **todo el código y la documentación** del proyecto
  para que se compile y ejecute en un equipo con permisos, y se dejó **SQLite**
  como proveedor de desarrollo para poder arrancar sin instalar nada.

---

## 1. Creación del proyecto

El proyecto se creó **manualmente** (archivos escritos a mano) porque en este
equipo no hay SDK. En un equipo con .NET 8 el comando equivalente habría sido:

```powershell
dotnet new webapi -n WebApi -o web_api --use-controllers
```

Estructura final creada:

```
web_api/
├─ WebApi.csproj
├─ Program.cs
├─ appsettings.json
├─ appsettings.Development.json
├─ .gitignore
├─ .vscode/
│   ├─ extensions.json
│   ├─ launch.json
│   └─ tasks.json
├─ Properties/
│   └─ launchSettings.json
├─ Models/
│   ├─ ApplicationUser.cs
│   ├─ Curso.cs
│   ├─ Estudiante.cs
│   ├─ Docente.cs
│   └─ Asignacion.cs
├─ Security/
│   └─ Roles.cs
├─ Data/
│   ├─ ApplicationDbContext.cs
│   ├─ DesignTimeDbContextFactory.cs
│   └─ DbSeeder.cs
├─ Dtos/
│   ├─ CursoDtos.cs
│   ├─ EstudianteDtos.cs
│   ├─ DocenteDtos.cs
│   ├─ AsignacionDtos.cs
│   └─ AuthDtos.cs
├─ Services/
│   └─ TokenService.cs
├─ Controllers/
│   ├─ AuthController.cs
│   ├─ CursosController.cs
│   ├─ DocentesController.cs
│   ├─ EstudiantesController.cs
│   └─ AsignacionesController.cs
└─ docs/
    ├─ REQUISITOS.md
    ├─ BASE_DE_DATOS.md
    └─ PASOS_PROYECTO.md
```

---

## 2. Paquetes NuGet (`WebApi.csproj`)

| Paquete | Versión | Para qué |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.10 | Autenticación con JWT |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.10 | Identity + roles sobre EF Core |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.10 | Proveedor SQLite (dev) |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.10 | Proveedor SQL Server (prod) |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.10 | Comandos `dotnet ef` |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.10 | Diseño/migraciones |
| `Swashbuckle.AspNetCore` | 6.6.2 | Swagger/OpenAPI |

Comando equivalente (referencia):

```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.10
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.10
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.10
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.10
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.10
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.10
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
```

---

## 3. Entidades (Code First)

Se tradujo el diagrama a clases en `Models/`:

1. **`Curso`** → `CursoId`, `Nombre`, navegación `Asignaciones`.
2. **`Estudiante`** → `EstudianteId`, `Nombres`, `Apellidos`, `Direccion`,
   navegación `Asignaciones`.
3. **`Docente`** → `DocenteId`, `Nombres`, `Apellidos`, navegación `Asignaciones`.
4. **`Asignacion`** → `AsignacionId` + tres FKs (`EstudianteId`, `CursoId`,
   `DocenteId`) y sus navegaciones.
5. **`ApplicationUser`** → extiende `IdentityUser` con `Nombres` y `Apellidos`.

Configuración Fluent API en `Data/ApplicationDbContext.cs`:
- Longitudes y campos obligatorios.
- Relaciones 1..* desde Curso/Estudiante/Docente hacia Asignacion.
- Índice único `(EstudianteId, CursoId)`.
- `DeleteBehavior.Restrict` para evitar cascadas múltiples en SQL Server.

---

## 4. Base de datos y migraciones

- `ApplicationDbContext` hereda de `IdentityDbContext<ApplicationUser>`.
- `DesignTimeDbContextFactory` permite que `dotnet ef` use el proveedor activo
  (`DatabaseProvider` en `appsettings.json`).
- **No se generaron migraciones en este equipo** (no hay SDK). El primer usuario
  con .NET debe ejecutar:

  ```powershell
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```

  Detalle completo en [`BASE_DE_DATOS.md`](BASE_DE_DATOS.md).

---

## 5. Seguridad: Identity + JWT con roles

### 5.1 Roles (`Security/Roles.cs`)
`Admin`, `Docente`, `Estudiante` (y constantes compuestas para los atributos
`[Authorize]`).

### 5.2 Usuarios semilla (`Data/DbSeeder.cs`)
Se crea un usuario por rol y datos de ejemplo. Al iniciar la app:

1. `MigrateAsync()` aplica migraciones.
2. Crea roles.
3. Crea usuarios (con `UserManager`) y les asigna su rol (`AddToRoleAsync`).
4. Inserta cursos, docentes, estudiantes y asignaciones si la BD está vacía.

### 5.3 JWT (`Services/TokenService.cs`)
- Firma HMAC-SHA256 con `Jwt:Key`.
- Incluye un claim `role` por cada rol del usuario.
- Devuelve token + fecha de expiración.

### 5.4 Autenticación (`Program.cs`)
- `AddIdentity<ApplicationUser, IdentityRole>()`.
- Esquema por defecto = `JwtBearerDefaults.AuthenticationScheme`.
- Validación de issuer, audience, lifetime y firma.
- Swagger con botón **Authorize (Bearer)**.

---

## 6. Controladores REST

| Controlador | Ruta base | GET | POST/PUT/DELETE |
|---|---|---|---|
| `AuthController` | `/api/auth` | `me` (autenticado) | `register`, `login` (anónimos) |
| `CursosController` | `/api/cursos` | Autenticado | **Admin** |
| `DocentesController` | `/api/docentes` | Autenticado | **Admin** |
| `EstudiantesController` | `/api/estudiantes` | **Admin, Docente** | **Admin** |
| `AsignacionesController` | `/api/asignaciones` | **Admin, Docente** | **Admin** |

### Endpoints (resumen)

```
POST   /api/auth/register        Registrar usuario (rol: Admin|Docente|Estudiante)
POST   /api/auth/login           Iniciar sesión -> token JWT
GET    /api/auth/me              Datos del usuario autenticado

GET    /api/cursos               Listar cursos
GET    /api/cursos/{id}          Obtener curso
POST   /api/cursos               Crear curso            [Admin]
PUT    /api/cursos/{id}          Actualizar curso       [Admin]
DELETE /api/cursos/{id}          Eliminar curso         [Admin]

GET    /api/docentes             Listar docentes
GET    /api/docentes/{id}        Obtener docente
POST   /api/docentes             Crear docente          [Admin]
PUT    /api/docentes/{id}        Actualizar docente     [Admin]
DELETE /api/docentes/{id}        Eliminar docente       [Admin]

GET    /api/estudiantes          Listar estudiantes     [Admin, Docente]
GET    /api/estudiantes/{id}     Obtener estudiante     [Admin, Docente]
POST   /api/estudiantes          Crear estudiante       [Admin]
PUT    /api/estudiantes/{id}     Actualizar estudiante  [Admin]
DELETE /api/estudiantes/{id}     Eliminar estudiante    [Admin]

GET    /api/asignaciones         Listar asignaciones    [Admin, Docente]
GET    /api/asignaciones/{id}    Obtener asignación     [Admin, Docente]
POST   /api/asignaciones         Crear asignación       [Admin]
PUT    /api/asignaciones/{id}    Actualizar asignación  [Admin]
DELETE /api/asignaciones/{id}    Eliminar asignación    [Admin]
```

Detalles REST aplicados:
- Códigos correctos: `200`, `201` (`CreatedAtAction`), `204`, `400`, `401`,
  `403`, `404`, `409`.
- DTOs de entrada/salida (no se exponen entidades directamente).
- Validación con Data Annotations + `[ApiController]` (respuestas 400 automáticas).

---

## 7. Configuración de VS Code

- `.vscode/extensions.json`: recomienda **C# Dev Kit** y **C#**.
- `.vscode/launch.json`: perfil de depuración `WebApi (http)`.
- `.vscode/tasks.json`: tareas `build`, `watch`, `ef-migrations-add`,
  `ef-database-update`.
- `Properties/launchSettings.json`: perfiles `http` (`:5080`) y `https`
  (`:7080`), con `ASPNETCORE_ENVIRONMENT=Development` y apertura de Swagger.

---

## 8. Cómo probar (en un equipo con permisos)

```powershell
cd web_api
dotnet restore
dotnet ef migrations add InitialCreate
dotnet run
```

Luego en `http://localhost:5080/swagger`:

1. `POST /api/auth/login` con `admin@webapi.local` / `Admin123*`.
2. Copia el `token` de la respuesta.
3. Pulsa **Authorize** y pega el token.
4. Prueba:
   - `GET /api/cursos` (debe funcionar).
   - `POST /api/cursos` con rol Admin (debe funcionar).
   - Inicia sesión con `estudiante@webapi.local` y prueba `GET /api/estudiantes`
     → debe devolver **403 Forbidden** (rol no autorizado).

---

## 9. Estado actual y pendientes

- [x] Código completo de la Web API (entidades, contexto, seeder, JWT,
      controladores, DTOs).
- [x] Configuración de proveedores SQLite/SQL Server.
- [x] Documentación (`REQUISITOS.md`, `BASE_DE_DATOS.md`, este archivo, `README.md`).
- [ ] **Pendiente en un equipo con permisos:** instalar .NET 8 y (opcional) SQL
      Server, ejecutar `dotnet restore`, generar la migración inicial y probar.
- [ ] **Pendiente:** cambiar las credenciales semilla y `Jwt:Key` para producción.

---

Ver también [`REQUISITOS.md`](REQUISITOS.md) y [`BASE_DE_DATOS.md`](BASE_DE_DATOS.md).
