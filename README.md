# Web API Cursos (Curso / Estudiante / Docente / Asignación)

Web API REST desarrollada con **.NET 8**, **Entity Framework Core (Code First)**,
**ASP.NET Core Identity** y **JWT con roles**. Modela el siguiente diagrama:

```
Curso 1──* Asignacion *──1 Estudiante
                 │*
                 │
                 1
              Docente
```

## Características

- **Code First**: entidades → migraciones → base de datos.
- **Doble proveedor de base de datos**: SQLite (desarrollo) y SQL Server
  (producción), conmutable por configuración.
- **Autenticación JWT** con **roles**: `Admin`, `Docente`, `Estudiante`.
- **CRUD REST** para Curso, Docente, Estudiante y Asignación.
- **Swagger** con botón *Authorize* para probar con token.
- **Siembra automática** de roles, usuarios y datos de ejemplo.
- **Documentación** detallada en `docs/`.

## Estructura

```
Models/       Entidades del dominio + ApplicationUser
Security/     Constantes de roles
Data/         DbContext, factory de diseño y seeder
Dtos/         Objetos de transferencia de entrada/salida
Services/     TokenService (generación de JWT)
Controllers/  Endpoints REST
docs/         Documentación del proyecto
```

## Inicio rápido

> Requisitos: **.NET 8 SDK**. Ver [`docs/REQUISITOS.md`](docs/REQUISITOS.md).

```powershell
cd web_api
dotnet restore
dotnet ef migrations add InitialCreate   # primera vez
dotnet run
```

Abre **http://localhost:5080/swagger**.

## Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Admin | `admin@webapi.local` | `Admin123*` |
| Docente | `docente@webapi.local` | `Docente123*` |
| Estudiante | `estudiante@webapi.local` | `Estudiante123*` |

## Matriz de permisos

| Recurso | Lectura | Escritura |
|---|---|---|
| `/api/cursos` | Autenticado | Admin |
| `/api/docentes` | Autenticado | Admin |
| `/api/estudiantes` | Admin, Docente | Admin |
| `/api/asignaciones` | Admin, Docente | Admin |

## Documentación

| Documento | Contenido |
|---|---|
| [`docs/REQUISITOS.md`](docs/REQUISITOS.md) | Qué instalar y cómo (incluye instalación sin permisos de admin) |
| [`docs/BASE_DE_DATOS.md`](docs/BASE_DE_DATOS.md) | Creación de la BD, migraciones y SQL Server vs SQLite |
| [`docs/PASOS_PROYECTO.md`](docs/PASOS_PROYECTO.md) | Bitácora paso a paso de cómo se construyó el proyecto |

## Nota sobre este equipo

Este proyecto se generó en un equipo **sin permisos de administrador**, donde no
se pudo instalar .NET ni SQL Server (error `1602`). El código y la documentación
quedan listos para compilarse y ejecutarse en un equipo con permisos. Ver
`docs/REQUISITOS.md`, sección 9.
