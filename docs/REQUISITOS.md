# Requisitos e instalación del entorno

Este documento explica **todo lo que un usuario nuevo necesita instalar** para
poder compilar, ejecutar y probar este proyecto (`Web API REST` con .NET 8,
EF Core Code First, Identity y JWT).

> Contexto importante: el equipo donde se generó este código **no tiene permisos
> de administrador**, por lo que no fue posible instalar el SDK ni la base de
> datos ahí. Todo el proyecto quedó escrito y documentado para que se compile y
> ejecute en un equipo donde **sí** se tengan permisos. Lee la sección
> ["Instalación sin permisos de administrador"](#instalación-sin-permisos-de-administrador)
> si tienes el mismo problema.

---

## 1. Resumen de software necesario

| Componente | ¿Obligatorio? | Para qué sirve | Versión mínima |
|---|---|---|---|
| **.NET SDK** | Sí | Compilar y ejecutar la Web API | 8.0 |
| **Visual Studio Code** | Sí (recomendado) | Editor del proyecto | Reciente |
| **Extensiones C# Dev Kit / C#** | Sí (si usas VS Code) | IntelliSense, depuración, ejecutar | Últimas |
| **EF Core CLI (`dotnet-ef`)** | Sí | Crear y aplicar migraciones | 8.0 |
| **SQL Server Express o LocalDB** | Opcional* | Base de datos para "producción" | 2019+ |
| **SQLite** | No se instala | Base de datos para desarrollo | Incluido por NuGet |
| **Git** | Opcional | Control de versiones | Cualquiera |
| **Cliente SQL (SSMS / Azure Data Studio)** | Opcional | Ver las tablas de SQL Server | Reciente |

\* SQL Server es **opcional** porque el proyecto puede ejecutarse de inmediato con
**SQLite** (modo desarrollo). Solo necesitas SQL Server si quieres trabajar con el
proveedor real. Ver `BASE_DE_DATOS.md`.

---

## 2. Instalar el .NET SDK 8

### Opción A: winget (Windows, requiere permisos / UAC)

```powershell
winget install --id Microsoft.DotNet.SDK.8 --source winget
```

Cuando aparezca la ventana de **UAC**, acepta. Si la rechazas o tu cuenta no es
administrador, verás el error **"Ha cancelado la instalación. Código 1602"**.

Verifica la instalación (abre una terminal nueva):

```powershell
dotnet --version      # debe mostrar 8.x.x
dotnet --list-sdks    # debe listar la versión 8
```

### Opción B: instalador gráfico

Descarga el instalador desde <https://dotnet.microsoft.com/download/dotnet/8.0>
y ejecútalo como administrador.

### Opción C: instalación local sin permisos de administrador

Ver la sección dedicada más abajo.

---

## 3. Extensiones de Visual Studio Code

El proyecto ya incluye `.vscode/extensions.json` con las recomendaciones. Al abrir
la carpeta, VS Code sugerirá instalarlas. Si no, instálalas manualmente:

- **C# Dev Kit** — id: `ms-dotnettools.csdevkit`
- **C#** — id: `ms-dotnettools.csharp`

Desde la terminal:

```powershell
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-dotnettools.csharp
```

---

## 4. Herramienta de migraciones de Entity Framework Core

Se instala como herramienta global (queda en tu perfil de usuario, no requiere
administrador):

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

Si ya la tienes instalada y quieres actualizarla:

```powershell
dotnet tool update --global dotnet-ef --version 8.*
```

Verifica:

```powershell
dotnet ef --version
```

> Si el comando `dotnet ef` no se reconoce, agrega a tu `PATH` la carpeta
> `%USERPROFILE%\.dotnet\tools` y abre una terminal nueva.

---

## 5. Verificación rápida del entorno

Ejecuta estos comandos dentro de la carpeta del proyecto:

```powershell
dotnet --version          # 8.x
dotnet ef --version       # 8.x
dotnet restore            # descarga los paquetes NuGet
dotnet build              # debe terminar en "Build succeeded"
```

Si los cuatro comandos funcionan, el entorno está listo.

---

## 6. Puesta en marcha en 5 pasos (usuario nuevo)

```powershell
# 1. Entrar a la carpeta del proyecto
cd web_api

# 2. Restaurar dependencias
dotnet restore

# 3. Crear la migración inicial (solo la primera vez)
dotnet ef migrations add InitialCreate

# 4. Ejecutar la API (usa SQLite por defecto, se crea webapi.db automáticamente)
dotnet run
```

5. Abre en el navegador: `http://localhost:5080/swagger`

Los datos (roles, usuarios y datos de ejemplo) se siembran solos al arrancar.
Consulta las credenciales en la sección 8.

---

## 7. Variables y claves de configuración relevantes

| Clave | Dónde | Descripción |
|---|---|---|
| `DatabaseProvider` | `appsettings.json` | `"Sqlite"` (dev) o `"SqlServer"` (prod) |
| `ConnectionStrings:SqliteConnection` | `appsettings.json` | Ruta del archivo `.db` |
| `ConnectionStrings:SqlServerConnection` | `appsettings.json` | Cadena de conexión a SQL Server |
| `Jwt:Key` | `appsettings.json` | Clave secreta de firma (mínimo 32 caracteres) |
| `Jwt:Issuer` / `Jwt:Audience` | `appsettings.json` | Emisor y audiencia del token |
| `Jwt:ExpiresMinutes` | `appsettings.json` | Vigencia del token en minutos |
| `ASPNETCORE_ENVIRONMENT` | Entorno | `Development` habilita Swagger |

En producción **no** dejes `Jwt:Key` en `appsettings.json`. Usa
[user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) o
variables de entorno, por ejemplo:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "una-clave-larga-y-secreta-de-produccion"
```

---

## 8. Usuarios semilla (creados automáticamente)

| Rol | Correo | Contraseña |
|---|---|---|
| Admin | `admin@webapi.local` | `Admin123*` |
| Docente | `docente@webapi.local` | `Docente123*` |
| Estudiante | `estudiante@webapi.local` | `Estudiante123*` |

> Cámbialos en `Data/DbSeeder.cs` antes de cualquier uso real.

---

## 9. Instalación sin permisos de administrador

Si no eres administrador del equipo, **no podrás** usar `winget install` para el
SDK ni instalar SQL Server. Alternativas:

### 9.1 Instalar el SDK .NET en tu carpeta de usuario

El script oficial `dotnet-install` no requiere permisos de administrador:

```powershell
# Descargar el script
Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile "$env:TEMP\dotnet-install.ps1"

# Instalar el SDK 8 en tu perfil (no toca carpetas del sistema)
& "$env:TEMP\dotnet-install.ps1" -Channel 8.0 -InstallDir "$env:USERPROFILE\.dotnet"
```

Luego agrega `%USERPROFILE%\.dotnet` al `PATH` de tu usuario (sin admin):

```powershell
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($userPath -notlike "*$env:USERPROFILE\.dotnet*") {
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$env:USERPROFILE\.dotnet", "User")
}
```

Abre una terminal **nueva** y comprueba `dotnet --version`.

> Nota: en el equipo donde se creó este proyecto esta instalación tampoco fue
> posible (restricciones de la máquina). Si ese es tu caso, ejecuta el proyecto
> en **otro equipo** o pide a soporte que habilite la instalación.

### 9.2 Evitar SQL Server: usar SQLite

SQLite no necesita instalación ni servidor. Deja `"DatabaseProvider": "Sqlite"`
en `appsettings.json` y todo funcionará sin permisos adicionales. Es la opción
recomendada cuando no puedes instalar software.

### 9.3 SQL Server sin permisos

- Usa **LocalDB** si ya viene instalado con Visual Studio.
- O solicita a soporte una instancia de SQL Server y solo configura la cadena de
  conexión (ver `BASE_DE_DATOS.md`). No requiere que tú instales nada.

---

## 10. Problemas frecuentes

| Síntoma | Causa probable | Solución |
|---|---|---|
| `dotnet` no se reconoce | SDK no instalado o `PATH` desactualizado | Reinstala / abre terminal nueva / revisa `PATH` |
| `dotnet ef` no se reconoce | Falta `dotnet-ef` o no está en `PATH` | `dotnet tool install --global dotnet-ef` y agrega `%USERPROFILE%\.dotnet\tools` |
| Error 1602 al instalar con winget | Rechazaste el UAC o no eres admin | Usa la instalación local (9.1) o pide permisos |
| `Build FAILED` por paquetes | Sin conexión o versiones distintas | `dotnet restore` con Internet; revisa `WebApi.csproj` |
| Swagger no aparece | `ASPNETCORE_ENVIRONMENT` no es `Development` | Ejecuta con `dotnet run` (usa `launchSettings.json`) |

---

Ver también:

- [`BASE_DE_DATOS.md`](BASE_DE_DATOS.md) — creación de la base de datos y migraciones.
- [`PASOS_PROYECTO.md`](PASOS_PROYECTO.md) — bitácora de cómo se construyó el proyecto.
- [`../README.md`](../README.md) — visión general.
