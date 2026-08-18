# Guía de inicio local desde cero

Esta guía permite ejecutar el sistema Ticketero completo en una máquina Windows nueva, sin contenedores:

- SQL Server local instalado en Windows.
- API .NET en `http://localhost:5000`.
- Aplicación Flutter para Windows.

El script [`iniciar-local.ps1`](./iniciar-local.ps1) automatiza el levantamiento y las migraciones. Docker es una alternativa opcional, no un requisito.

## 1. Requisitos

Instala los siguientes programas y verifica que queden disponibles desde una nueva ventana de PowerShell.

| Programa | Versión requerida | Comprobación |
| --- | --- | --- |
| Git | Cualquiera reciente | `git --version` |
| SQL Server Express o Developer | 2022 o posterior | abre SQL Server Configuration Manager o SSMS |
| .NET SDK | 10.0 | `dotnet --list-sdks` |
| Flutter SDK | 3.22 o superior con soporte Windows | `flutter --version` |
| Visual Studio 2022 | Community o superior, carga **Desarrollo de escritorio con C++** | necesario para compilar Flutter para Windows |

Después de instalar Flutter ejecuta:

```powershell
flutter doctor
flutter config --enable-windows-desktop
```

Corrige cualquier error que indique `flutter doctor`, especialmente los de Visual Studio o Windows desktop. Cierra y vuelve a abrir PowerShell tras modificar el `PATH` durante una instalación.

Durante la instalación de SQL Server selecciona **Database Engine Services** y conserva o crea la instancia `SQLEXPRESS`. La autenticación de Windows del usuario actual debe tener permisos para crear una base de datos. SQL Server Management Studio (SSMS) es opcional, pero útil para inspeccionar `DBTicketero`.

## 2. Obtener el proyecto

Clona el repositorio y abre PowerShell en la carpeta raíz:

```powershell
git clone <URL_DEL_REPOSITORIO> App_Ticketero
cd App_Ticketero
```

Si recibiste una copia ZIP, extráela y abre PowerShell dentro de la carpeta que contiene `iniciar-local.ps1`.

## 3. Preparar SQL Server local

1. Abre SQL Server Configuration Manager y confirma que el servicio **SQL Server (SQLEXPRESS)** esté iniciado.
2. Si instalaste una instancia con otro nombre, anótalo. Se usará al ejecutar el script.
3. Opcionalmente, comprueba la conexión con SSMS usando el servidor `localhost\SQLEXPRESS` y autenticación de Windows.

No necesitas crear manualmente la base de datos: las migraciones crearán `DBTicketero` en el primer arranque.

## 4. Iniciar todo el sistema

Desde la raíz del repositorio ejecuta:

```powershell
.\iniciar-local.ps1
```

El modo predeterminado usa `.\SQLEXPRESS`, sin Docker ni Redis. Si tu instancia tiene otro nombre, indícala así:

```powershell
.\iniciar-local.ps1 -SqlServerInstance 'localhost\MI_INSTANCIA'
```

Si Windows bloquea la ejecución de scripts descargados, ejecuta únicamente para la sesión actual:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\iniciar-local.ps1
```

En el primer inicio el script:

1. Se conecta a SQL Server local.
2. Aplica las migraciones de Entity Framework y crea `DBTicketero`.
3. Descarga las dependencias de Flutter.
4. Abre dos ventanas de PowerShell: una con la API y otra con Flutter para Windows.

La API se ejecuta en modo `Development`; al abrirse carga los datos iniciales, incluyendo el usuario `admin` con contraseña `Admin123!`.

El primer inicio puede tardar algunos minutos mientras se descargan dependencias de .NET y Flutter. Los inicios posteriores son más rápidos.

## 5. Verificar el resultado

Con las dos ventanas abiertas, comprueba:

```powershell
Invoke-RestMethod http://localhost:5000/healthz
Invoke-RestMethod http://localhost:5000/ready
```

Ambos endpoints deben responder correctamente. También puedes abrir Swagger en [http://localhost:5000/swagger](http://localhost:5000/swagger).

La ventana de Flutter debe mostrar la aplicación de escritorio. Inicia sesión con:

```text
Usuario: admin
Contraseña: Admin123!
```

## 6. Opciones del lanzador

Por defecto Flutter se ejecuta en Windows:

```powershell
.\iniciar-local.ps1
```

Para usar otro dispositivo reconocido por Flutter, por ejemplo Chrome:

```powershell
.\iniciar-local.ps1 -Device chrome
```

Para iniciar los servicios sin volver a aplicar las migraciones:

```powershell
.\iniciar-local.ps1 -SkipMigrations
```

Consulta los destinos disponibles con `flutter devices`.

## 7. Detener el sistema

1. Cierra las ventanas de PowerShell de la API y Flutter, o presiona `Ctrl+C` dentro de cada una.
2. SQL Server es un servicio local y puede quedar iniciado. No afecta el siguiente arranque del proyecto.

Para volver a iniciar, ejecuta otra vez `./iniciar-local.ps1`.

## 8. Reiniciar la base de datos local

> Advertencia: este procedimiento borra definitivamente los datos locales de Ticketero almacenados en SQL Server.

Desde SSMS, conéctate a tu instancia local y elimina la base de datos `DBTicketero`. También puedes hacerlo desde PowerShell si tienes `sqlcmd` instalado:

```powershell
sqlcmd -S .\SQLEXPRESS -E -Q "ALTER DATABASE DBTicketero SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE DBTicketero;"
```

Después ejecuta `./iniciar-local.ps1` sin `-SkipMigrations`. Las migraciones y los datos iniciales se crearán de nuevo.

## 9. Problemas frecuentes

### No es posible conectar a SQL Server

Confirma que el servicio `SQL Server (SQLEXPRESS)` está iniciado y que la instancia indicada en `-SqlServerInstance` es correcta. Si el usuario actual no puede crear la base, solicita permisos `dbcreator` al administrador de SQL Server.

### `dotnet ef` no se reconoce

Instala la herramienta de Entity Framework y vuelve a abrir PowerShell:

```powershell
dotnet tool install --global dotnet-ef
```

Si ya estaba instalada, actualízala con `dotnet tool update --global dotnet-ef`.

### Flutter no encuentra el dispositivo Windows

Ejecuta `flutter doctor -v` y confirma que Visual Studio tiene instalada la carga **Desarrollo de escritorio con C++**. Luego ejecuta `flutter config --enable-windows-desktop` y reinicia la terminal.

### El puerto 5000 está ocupado

Identifica el proceso y ciérralo si corresponde:

```powershell
Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue |
    Select-Object LocalPort, State, OwningProcess
```

No modifiques el puerto sin actualizar también la URL de Flutter.

## 10. Alternativa con Docker (opcional)

Si prefieres no instalar SQL Server en Windows, instala Docker Desktop y ejecuta:

```powershell
.\iniciar-local.ps1 -DatabaseMode Docker
```

Este modo inicia SQL Server y Redis en contenedores y mantiene la misma API y aplicación Flutter locales. Para detener sus contenedores:

```powershell
docker compose -f .\ApiTiketero\Ticketero\docker-compose.yml stop sqlserver redis
```
