# Base de datos: creación y migraciones (Code First)

Este documento explica **cómo se crea y administra la base de datos** del
proyecto usando el enfoque **Code First** de Entity Framework Core: a partir de
las clases del modelo se generan las tablas mediante **migraciones**.

> En el equipo donde se generó el proyecto **no se pudo instalar SQL Server**
> (sin permisos). Por eso aquí dejamos **todas las instrucciones** para hacerlo
> en otro equipo. Mientras tanto, el proyecto funciona con **SQLite** sin
> instalar nada.

---

## 1. Modelo de datos (según el diagrama)

Cuatro tablas de negocio + las tablas de Identity (usuarios/roles).

### `Curso`
| Columna | Tipo | Notas |
|---|---|---|
| `CursoId` | int | Clave primaria (identidad) |
| `Nombre` | nvarchar(150) | Obligatorio |

### `Estudiante`
| Columna | Tipo | Notas |
|---|---|---|
| `EstudianteId` | int | Clave primaria |
| `Nombres` | nvarchar(100) | Obligatorio |
| `Apellidos` | nvarchar(100) | Obligatorio |
| `Direccion` | nvarchar(200) | Opcional |

### `Docente`
| Columna | Tipo | Notas |
|---|---|---|
| `DocenteId` | int | Clave primaria |
| `Nombres` | nvarchar(100) | Obligatorio |
| `Apellidos` | nvarchar(100) | Obligatorio |

### `Asignacion` (tabla intermedia con 3 claves foráneas)
| Columna | Tipo | Notas |
|---|---|---|
| `AsignacionId` | int | Clave primaria |
| `EstudianteId` | int | FK → `Estudiante` |
| `CursoId` | int | FK → `Curso` |
| `DocenteId` | int | FK → `Docente` |

### Relaciones
```
Curso      1 ──── * Asignacion * ──── 1 Estudiante
                          │
                          │ *
                          │
                          1
                        Docente
```
- Un `Curso` tiene muchas `Asignacion`.
- Un `Estudiante` tiene muchas `Asignacion`.
- Un `Docente` tiene muchas `Asignacion`.
- Índice único `(EstudianteId, CursoId)`: un estudiante no puede repetir curso.
- Borrado en cascada **deshabilitado** (`DeleteBehavior.Restrict`) para evitar
  "multiple cascade paths" en SQL Server.

La configuración está en `Data/ApplicationDbContext.cs`.

### Tablas de Identity
Al heredar de `IdentityDbContext<ApplicationUser>` se crean además:
`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`,
`AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`.

---

## 2. Proveedores soportados

El proyecto soporta **dos proveedores** conmutables por configuración:

| Proveedor | Uso | Requiere instalar algo |
|---|---|---|
| **SQLite** | Desarrollo / pruebas rápidas | No |
| **SQL Server** (Express / LocalDB) | Producción / entorno real | Sí |

El selector está en `appsettings.json`:

```json
"DatabaseProvider": "Sqlite"
```

Cambia a `"SqlServer"` para usar SQL Server.

> ⚠️ **Las migraciones son específicas de cada proveedor.** Si cambias de
> proveedor debes regenerar la carpeta `Migrations/` (ver sección 6).

---

## 3. Opción rápida: SQLite (sin instalar nada)

1. Asegúrate de tener `"DatabaseProvider": "Sqlite"`.
2. Genera la migración inicial:

   ```powershell
   dotnet ef migrations add InitialCreate
   ```

3. Ejecuta la API. Al iniciar, `DbSeeder` aplica las migraciones y crea
   `webapi.db`:

   ```powershell
   dotnet run
   ```

   También puedes aplicar la migración manualmente:

   ```powershell
   dotnet ef database update
   ```

Listo. No hace falta instalar ningún motor de base de datos.

---

## 4. Instalar SQL Server Express / LocalDB

Elige **una** de las opciones.

### Opción A: SQL Server Express (con winget, requiere permisos)

```powershell
winget install --id Microsoft.SQLServer.2022.Express --source winget
```

### Opción B: LocalDB (ligero, viene con Visual Studio)

Si tienes Visual Studio, LocalDB suele venir incluido. Verifícalo:

```powershell
sqllocaldb info
```

Si no aparece, descarga "SQL Server Express LocalDB" desde
<https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb>.

### Opción C: SQL Server Developer (completo, con SSMS)

Descarga desde <https://www.microsoft.com/sql-server/sql-server-downloads>.

### Verificar la instancia

```powershell
# LocalDB
sqllocaldb start MSSQLLocalDB
sqllocaldb info MSSQLLocalDB

# Express (servicio)
Get-Service -Name 'MSSQL*'
```

---

## 5. Cadena de conexión

Edita `appsettings.json` → `ConnectionStrings:SqlServerConnection`.

**LocalDB** (recomendado para desarrollo local):

```
Server=(localdb)\MSSQLLocalDB;Database=WebApiCursos;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

**Express** (instancia por defecto `.\SQLEXPRESS`):

```
Server=.\SQLEXPRESS;Database=WebApiCursos;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

**Con usuario y contraseña** (si tu instancia usa autenticación SQL):

```
Server=MI_SERVIDOR;Database=WebApiCursos;User Id=sa;Password=TU_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True
```

> `TrustServerCertificate=True` evita errores de certificado en entornos locales.

---

## 6. Crear la base de datos con migraciones (SQL Server)

### 6.1 Cambiar el proveedor

En `appsettings.json`:

```json
"DatabaseProvider": "SqlServer"
```

### 6.2 Regenerar migraciones (¡obligatorio al cambiar de proveedor!)

Las migraciones anteriores (de SQLite) no sirven para SQL Server. Bórralas y
genera nuevas:

```powershell
# 1. Eliminar la carpeta de migraciones de SQLite
Remove-Item -Recurse -Force .\Migrations

# 2. Crear la migración para SQL Server
dotnet ef migrations add InitialCreate

# 3. Crear/actualizar la base de datos
dotnet ef database update
```

> El `DesignTimeDbContextFactory` (`Data/DesignTimeDbContextFactory.cs`) lee
> `DatabaseProvider` de `appsettings.json`, por lo que `dotnet ef` respeta el
> proveedor activo automáticamente.

### 6.3 Comandos útiles de EF Core

```powershell
# Listar migraciones
dotnet ef migrations list

# Ver el SQL que se ejecutará (sin aplicarlo)
dotnet ef migrations script

# Revertir a una migración anterior
dotnet ef database update NombreMigracion

# Eliminar la última migración (si aún no se aplicó)
dotnet ef migrations remove

# Eliminar la base de datos por completo
dotnet ef database drop
```

### 6.4 Alternativa: generar el script SQL y ejecutarlo manualmente

Útil cuando no puedes conectar con `dotnet ef` (por ejemplo, en otro servidor):

```powershell
dotnet ef migrations script -o create_database.sql
```

Luego ejecuta `create_database.sql` en SQL Server Management Studio (SSMS) o con
`sqlcmd`:

```powershell
sqlcmd -S .\SQLEXPRESS -i create_database.sql
```

---

## 7. Siembra de datos (seeding)

Al arrancar la aplicación, `Data/DbSeeder.cs`:

1. Aplica las migraciones pendientes (`context.Database.MigrateAsync()`).
2. Crea los roles **Admin**, **Docente** y **Estudiante**.
3. Crea un usuario por rol (ver `REQUISITOS.md`, sección 8).
4. Inserta cursos, docentes, estudiantes y asignaciones de ejemplo (solo si la
   tabla `Cursos` está vacía).

No necesitas ejecutar nada extra: todo ocurre al hacer `dotnet run`.

---

## 8. Resumen de comandos por escenario

| Escenario | Comandos |
|---|---|
| Primer arranque con SQLite | `dotnet ef migrations add InitialCreate` → `dotnet run` |
| Cambiar de SQLite a SQL Server | Cambiar `DatabaseProvider` → `Remove-Item -Recurse .\Migrations` → `dotnet ef migrations add InitialCreate` → `dotnet ef database update` |
| Agregar un campo a una entidad | Editar la clase → `dotnet ef migrations add NombreCambio` → `dotnet ef database update` |
| Volver a SQLite | Cambiar `DatabaseProvider` → borrar `Migrations/` → regenerar |

---

## 9. Problemas frecuentes

| Síntoma | Causa | Solución |
|---|---|---|
| `A network-related or instance-specific error` | No se encuentra la instancia | Revisa el `Server=` de la cadena; inicia el servicio/LocalDB |
| `Cannot open database "WebApiCursos"` | La BD no existe | `dotnet ef database update` |
| `There is already an object named 'AspNetUsers'` | Migración de otro proveedor aplicada | Borra `Migrations/` y regenera para el proveedor correcto |
| `Introducing FOREIGN KEY constraint ... may cause cycles or multiple cascade paths` | Cascadas múltiples | Ya resuelto con `DeleteBehavior.Restrict` |
| `Login failed for user` | Autenticación SQL | Usa `Trusted_Connection=True` o revisa usuario/contraseña |
| `The certificate chain was issued by an authority that is not trusted` | Certificado local | Agrega `TrustServerCertificate=True` |
| El `.db` de SQLite no se crea | Proveedor mal configurado | Verifica `DatabaseProvider` y la cadena SQLite |

---

## 10. Ubicación de los archivos clave

| Archivo | Responsabilidad |
|---|---|
| `Models/*.cs` | Entidades del dominio |
| `Data/ApplicationDbContext.cs` | Mapeo, relaciones y DbSets |
| `Data/DesignTimeDbContextFactory.cs` | Proveedor usado por `dotnet ef` |
| `Data/DbSeeder.cs` | Migración + roles + usuarios + datos de ejemplo |
| `appsettings.json` | Proveedor y cadenas de conexión |
| `Migrations/` | Migraciones generadas (no editar a mano) |

Ver también [`REQUISITOS.md`](REQUISITOS.md) y [`PASOS_PROYECTO.md`](PASOS_PROYECTO.md).
