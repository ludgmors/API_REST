# 📖 Guía de usuario — Web API de Cursos

Este documento explica **qué hace este proyecto, cómo está organizado y cómo usarlo**, pensado para alguien que no tiene conocimientos de programación.

---

## 1. ¿Qué es este proyecto?

Imagina que tienes una escuela o instituto y necesitas llevar el control de:

- **Cursos** que se imparten (por ejemplo: "Programación I", "Base de Datos").
- **Docentes** que enseñan esos cursos.
- **Estudiantes** que se inscriben.
- **Asignaciones**, que son la relación entre un estudiante, un curso y el docente que lo imparte.

Este proyecto es una **API REST**. Es como un "servicio en la nube" (pero que corre en tu computadora) que permite a otra aplicación (una app móvil, una página web, etc.) consultar y modificar esa información a través de Internet.

Incluye:

- **Seguridad con usuario y contraseña** (solo quien esté autenticado puede usarla).
- **Roles de usuario**: Admin, Docente y Estudiante (cada uno puede hacer cosas distintas).
- **Base de datos SQL Server** donde se guarda toda la información.
- **Swagger**: una página web para probar la API desde el navegador sin programar nada.

---

## 2. ¿Cómo está organizado el proyecto? (carpetas y archivos)

Aquí te explico cada carpeta y archivo de forma sencilla:

### 📁 Carpeta raíz (nivel principal)

| Archivo | ¿Qué hace? |
|---------|------------|
| `Program.cs` | Es el "cerebro" del programa. Aquí se configura todo: la base de datos, la seguridad, los tokens de acceso y se inicia la aplicación. |
| `WebApi.csproj` | Lista de librerías y herramientas que necesita el proyecto para funcionar (paquetes NuGet). |
| `appsettings.json` | Archivo de configuración. Aquí se dice que se use SQL Server, la clave secreta para los tokens, etc. |
| `appsettings.Development.json` | Configuración extra solo para cuando se prueba en modo desarrollo. |
| `Properties/launchSettings.json` | Configura los puertos y la URL donde se ejecuta la API (por defecto `http://localhost:5080`). |
| `.gitignore` | Indica qué archivos no se deben subir a un repositorio (por ejemplo, la base de datos local). |
| `README.md` | Resumen rápido del proyecto. |
| `GUIA_USUARIO.md` | Este archivo que estás leyendo ahora. |

---

### 📁 Carpeta `Models/` — Las entidades (qué se guarda en la base de datos)

Cada archivo define una "tabla" de la base de datos:

| Archivo | ¿Qué representa? | Campos principales |
|---------|------------------|--------------------|
| `Curso.cs` | Un curso ofrecido | Id, Nombre |
| `Docente.cs` | Un profesor | Id, Nombres, Apellidos |
| `Estudiante.cs` | Un alumno | Id, Nombres, Apellidos, Dirección |
| `Asignacion.cs` | Relación entre estudiante, curso y docente | Id, EstudianteId, CursoId, DocenteId |
| `ApplicationUser.cs` | Usuario del sistema (para login) | Id, Email, Nombres, Apellidos, contraseña |

---

### 📁 Carpeta `Security/` — Roles de usuario

| Archivo | ¿Qué hace? |
|---------|------------|
| `Roles.cs` | Define los tres roles existentes: `Admin`, `Docente` y `Estudiante`. Se usa para saber qué puede hacer cada quien. |

---

### 📁 Carpeta `Data/` — Base de datos y configuración

| Archivo | ¿Qué hace? |
|---------|------------|
| `ApplicationDbContext.cs` | Conecta el programa con la base de datos SQL Server. Define las tablas y sus relaciones. |
| `DbSeeder.cs` | Al iniciar la aplicación, crea automáticamente los roles, los usuarios de prueba y datos de ejemplo (cursos, docentes, estudiantes y asignaciones). |
| `DesignTimeDbContextFactory.cs` | Ayuda a las herramientas de base de datos a saber qué proveedor usar (SQL Server o SQLite). |

---

### 📁 Carpeta `Dtos/` — Datos que entran y salen

Los DTO son "envases" de datos. El programa no entrega directamente la información de la base de datos, sino versiones simplificadas para enviar por Internet:

| Archivo | ¿Qué hace? |
|---------|------------|
| `AuthDtos.cs` | Datos para registrarse, iniciar sesión y recibir el token de acceso. |
| `CursoDtos.cs` | Datos de entrada/salida para cursos. |
| `DocenteDtos.cs` | Datos de entrada/salida para docentes. |
| `EstudianteDtos.cs` | Datos de entrada/salida para estudiantes. |
| `AsignacionDtos.cs` | Datos de entrada/salida para asignaciones. |

---

### 📁 Carpeta `Services/` — Servicios internos

| Archivo | ¿Qué hace? |
|---------|------------|
| `TokenService.cs` | Genera los "tokens JWT". Son como llaves digitales que prueban que un usuario está autenticado y qué rol tiene. |

---

### 📁 Carpeta `Controllers/` — Endpoints (las puertas de entrada)

Cada archivo es un conjunto de funciones que responden a peticiones desde el navegador u otra aplicación:

| Archivo | Ruta | ¿Qué permite hacer? |
|---------|------|---------------------|
| `AuthController.cs` | `/api/auth` | Registrarse, iniciar sesión y ver datos del usuario logueado. |
| `CursosController.cs` | `/api/cursos` | Listar, ver, crear, editar y eliminar cursos. |
| `DocentesController.cs` | `/api/docentes` | Listar, ver, crear, editar y eliminar docentes. |
| `EstudiantesController.cs` | `/api/estudiantes` | Listar, ver, crear, editar y eliminar estudiantes. |
| `AsignacionesController.cs` | `/api/asignaciones` | Listar, ver, crear, editar y eliminar asignaciones. |

---

### 📁 Carpeta `Migrations/` — Historial de la base de datos

Contiene los archivos que le dicen a SQL Server cómo crear las tablas. Se generó automáticamente al iniciar el proyecto por primera vez.

---

### 📁 Carpeta `docs/` — Documentación técnica

| Archivo | Contenido |
|---------|-----------|
| `REQUISITOS.md` | Qué instalar para usar el proyecto (.NET SDK, etc.). |
| `BASE_DE_DATOS.md` | Cómo funciona la base de datos y cómo cambiar entre SQL Server y SQLite. |
| `PASOS_PROYECTO.md` | Bitácora de cómo se construyó el proyecto. |

---

### 📁 Carpeta `.vscode/` — Configuración del editor

Archivos para que Visual Studio Code reconozca el proyecto y se pueda depurar fácilmente.

---

## 3. ¿Cómo usar el proyecto?

### Paso 0: Requisitos
Necesitas tener instalado en tu computadora:
- **.NET 8 SDK** (para compilar y ejecutar).
- **SQL Server LocalDB** (ya lo tienes instalado).

### Paso 1: Abrir la terminal
Abre PowerShell o la terminal de comandos y entra a la carpeta del proyecto:

```powershell
cd C:\Users\Ludwi\Downloads\web_api_01\web_api
```

*(Nota: si la carpeta se llama diferente, ajusta la ruta.)*

### Paso 2: Ejecutar la aplicación

```powershell
dotnet run
```

Verás algo como:
```
Now listening on: http://localhost:5080
```

Eso significa que la API ya está corriendo en tu computadora.

### Paso 3: Abrir Swagger (para probar sin programar)

Abre tu navegador web y escribe:

```
http://localhost:5080/swagger
```

Verás una página con todos los endpoints disponibles. Ahí puedes hacer clic en cada uno y probarlo.

---

## 4. Usuarios de prueba (credenciales)

Al iniciar la aplicación, se crean automáticamente estos usuarios:

| Rol | Correo | Contraseña | ¿Qué puede hacer? |
|-----|--------|------------|-------------------|
| **Admin** | `admin@webapi.local` | `Admin123*` | Todo: leer, crear, editar y eliminar cursos, docentes, estudiantes y asignaciones. |
| **Docente** | `docente@webapi.local` | `Docente123*` | Solo puede leer (ver) cursos, docentes, estudiantes y asignaciones. No puede crear ni borrar. |
| **Estudiante** | `estudiante@webapi.local` | `Estudiante123*` | No tiene acceso a los endpoints de cursos/docentes/estudiantes/asignaciones (solo puede ver su propio perfil en `/api/auth/me`). |

---

## 5. Cómo probar en Swagger (paso a paso)

1. Abre `http://localhost:5080/swagger`.
2. Busca `POST /api/auth/login` y haz clic en **Try it out**.
3. En el cuerpo de la petición escribe:
   ```json
   {
     "email": "admin@webapi.local",
     "password": "Admin123*"
   }
   ```
4. Haz clic en **Execute**. Te devolverá un **token** (una cadena larga de texto).
5. Copia ese token.
6. Arriba a la derecha, haz clic en el botón **Authorize** (o un candado).
7. Pega el token (solo el texto, sin la palabra "Bearer") y haz clic en **Authorize**.
8. Ya puedes probar los demás endpoints (`/api/cursos`, `/api/estudiantes`, etc.) con permisos de Admin.

---

## 6. Matriz de permisos (resumen)

| Recurso | ¿Quién puede leer? | ¿Quién puede escribir (crear/editar/borrar)? |
|---------|-------------------|---------------------------------------------|
| `/api/cursos` | Cualquier usuario autenticado | Solo Admin |
| `/api/docentes` | Cualquier usuario autenticado | Solo Admin |
| `/api/estudiantes` | Admin y Docente | Solo Admin |
| `/api/asignaciones` | Admin y Docente | Solo Admin |

---

## 7. ¿Dónde está la base de datos?

La base de datos se llama **`WebApiCursos`** y está en tu **SQL Server LocalDB**:

- **Servidor:** `(localdb)\MSSQLLocalDB`
- **Base de datos:** `WebApiCursos`

Puedes verla con **SQL Server Management Studio (SSMS)** o **Azure Data Studio** conectándote a `(localdb)\MSSQLLocalDB` con autenticación de Windows.

Las tablas que encontrarás son:
- `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` (usuarios y roles de seguridad)
- `Cursos`, `Docentes`, `Estudiantes`, `Asignaciones` (datos de la escuela)

---

## 8. Comandos útiles

| Comando | Para qué sirve |
|---------|----------------|
| `dotnet run` | Inicia la API en `http://localhost:5080`. |
| `dotnet build` | Compila el proyecto (verifica que no haya errores). |
| `dotnet restore` | Descarga las librerías necesarias. |

---

## 9. Solución de problemas comunes

| Problema | Solución |
|----------|----------|
| No se reconoce el comando `dotnet` | Instala .NET 8 SDK. |
| La base de datos no se creó | Ejecuta `dotnet ef database update` (necesitas tener instalada la herramienta `dotnet-ef`). |
| No puedo hacer login | Verifica que estés usando el correo y contraseña exactos de la tabla de usuarios de prueba. |
| Recibo error 403 (Prohibido) | Tu rol no tiene permiso para esa acción. Usa el usuario Admin para acciones de escritura. |
| Swagger no aparece | Asegúrate de ejecutar `dotnet run` y entrar a `/swagger`. |
| No puedo conectar a SQL Server | Verifica que la instancia `(localdb)\MSSQLLocalDB` esté iniciada. Ejecuta: `sqllocaldb start MSSQLLocalDB` |

---

## 10. Resumen rápido

1. Ejecuta `dotnet run`.
2. Abre `http://localhost:5080/swagger`.
3. Haz login con `admin@webapi.local` / `Admin123*`.
4. Copia el token y pégalo en **Authorize**.
5. Prueba los endpoints de cursos, docentes, estudiantes y asignaciones.

¡Con eso ya puedes usar y probar toda la API!