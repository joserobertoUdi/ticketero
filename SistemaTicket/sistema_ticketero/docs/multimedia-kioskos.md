# Multimedia de kioskos (logo y video de fondo)

Los archivos multimedia se suben al **servicio central de gestión documental**
(SharepointApi, `http://10.1.210.10/SharepointApi`) en lugar de guardarse como
rutas locales. Así el mismo video o logo es accesible desde cualquier kiosko.

---

## Dónde está

**Configuración → Multimedia** (`KioskoMediaPanel`).

Hasta ahora este panel existía en el código pero no estaba registrado en
`settings_screen.dart`, por lo que era inalcanzable desde la interfaz.

Desde cada tarjeta de kiosko, el botón **⚙ Configurar multimedia** abre el
diálogo de subida.

---

## Flujo

```
Usuario elige archivo
   ↓
POST http://10.1.210.10/SharepointApi/api/Archivos   (multipart/form-data)
   Contenedor      = "Ticketero"
   Archivo         = binario
   EntidadOrigen   = "KioskoMultimedia"
   ReferenciaOrigen= "kiosko-{id}"
   UsuarioRegistro = nombreUsuario del operador
   Header: ProviderKey
   ↓
Respuesta 201 (ApiSuccessResponse<ArchivoDto>):
   { "exito": true,
     "mensaje": "Archivo guardado correctamente.",
     "datos": { "uid": "...", "nombreArchivo": "...", "contentType": "...",
                "tamanoBytes": 0, "urlWeb": "...", ... } }
   ↓
Se guarda la referencia  sharepoint:{uid}
   ├── en SharedPreferences (configuración local del kiosko)
   └── en POST /api/ConfiguracionMultimedia de la API Ticketero
       (tipoContenido = "Logo" | "Video", rutaArchivo = la referencia)
   ↓
Al reproducir:
   GET /api/Archivos/{uid}/contenido  con la cabecera ProviderKey
```

### Por qué `sharepoint:{uid}` y no la URL completa

Si se guardara `http://10.1.210.10/SharepointApi/api/Archivos/{uid}/contenido`,
un cambio de IP o de ruta del servicio invalidaría la configuración de todos los
kioskos. Guardando solo el uid, basta con cambiar `SharepointConstants.baseUrl`.

`SharepointConstants.resolveUrl()` traduce la referencia a URL en el momento de
usarla, y deja pasar sin tocar las rutas locales y las URLs externas, que siguen
funcionando para configuraciones antiguas.

---

## Archivos

| Archivo | Función |
|---|---|
| `core/constants/sharepoint_constants.dart` | URL base, ProviderKey, contenedor, extensiones y límite de tamaño |
| `data/datasources/remote/archivo_remote_datasource.dart` | Sube a SharepointApi, informa del progreso y traduce los errores |
| `data/datasources/remote/multimedia_remote_datasource.dart` | Registra la referencia en `ConfiguracionMultimedia` de la API Ticketero |
| `presentation/screens/settings/widgets/kiosko_media_panel.dart` | UI del panel |
| `core/utils/image_utils.dart` | Resuelve referencias al construir el `ImageProvider` |
| `.../widgets/background_video_widget.dart` | Reproduce el video con la cabecera |

---

## Requisitos del lado de SharepointApi

Verificados leyendo el código fuente en `C:\proyectos\activeRepository\SharePointApi`.

| Requisito | Detalle |
|---|---|
| **Contenedor registrado** | `Ticketero` debe existir en la tabla `Contenedores` de `DbSharePoint` con `Activo = 1`. Si no, responde **400** con `ContenedorNoConfiguradoException`. |
| **API Key** | El header es `ProviderKey`. En `appsettings.json` ya existe la ranura `ApiKeys:ticketero-api`, que es la que identifica a este sistema como `AplicacionOrigen`. |
| **Tamaño** | Tope global `Limites:TamanoMaximoPeticionMb` = **60 MB**, aplicado a Kestrel, IIS y `FormOptions`. Además cada contenedor tiene su propio `TamanoMaximoMb`, que puede ser menor. |
| **Extensiones** | `PoliticaExtensiones` bloquea siempre `.exe .dll .msi .scr .com .bat .cmd .ps1 .sh .vbs .jar .html .htm .hta .svg .xhtml .sql`. Encima, si el contenedor define `ExtensionesPermitidas`, actúa como lista blanca. |
| **Rate limit** | `SubidaLimiter`: 60 peticiones/minuto por cliente. `ConsultaLimiter`: 200/minuto. |

Los errores llegan siempre como `ApiErrorResponse` con el motivo en `mensaje`;
`archivo_remote_datasource.dart` lo extrae y lo muestra tal cual, en lugar de
inventar un texto propio.

---

## Limitaciones conocidas

### El registro en la API puede fallar por la clave foránea

`ConfiguracionMultimedia.KioskoId` es una **FK obligatoria** hacia `dbo.Kiosko`
(confirmado en el snapshot de EF Core: `.HasForeignKey("KioskoId").IsRequired()`).

Pero `KioskoMedia.id` en Flutter se genera localmente (`maxId + 1` en
`kiosko_media_panel.dart`) y se guarda en SharedPreferences. **No tiene relación
con el `KioskoId` real de la base de datos.**

Consecuencia: si el id local no coincide con un kiosko existente, el `POST` a
`/api/ConfiguracionMultimedia` devuelve un error de integridad referencial.

El código está preparado para eso: **la subida y la configuración local se
completan igual**, y el diálogo avisa de que no pudo registrarse en la API. El
archivo ya está en el servicio central y el kiosko lo reproduce; lo que no
ocurre es la sincronización con los demás kioskos.

**Para resolverlo de raíz** hay que hacer que el panel trabaje sobre los kioskos
reales de la API (`GET /api/Kioskos` o `/api/kioskos-fisicos`) en lugar de una
lista local. Es un refactor de `SettingsProvider.kioskoLocations`.

### Los videos pueden no caber

El tope de petición son 60 MB. Un video de fondo en 1080p supera eso con
facilidad. Si aparece el error de tamaño hay tres salidas: recomprimir el video,
subir `Limites:TamanoMaximoPeticionMb` en el `appsettings` de SharepointApi, o
ampliar el `TamanoMaximoMb` del contenedor en la tabla `Contenedores`.

### La ProviderKey está en el código

`SharepointConstants.providerKey` es una constante compilada en el binario que
se distribuye a cada kiosko. Cualquiera con acceso al ejecutable puede
extraerla. La alternativa es enrutar la subida por la API de Ticketero, para que
la clave nunca salga del servidor.

### El endpoint de upload propio quedó sin uso

`POST /api/ConfiguracionMultimedia/upload` de la API Ticketero (que guarda en
`wwwroot/uploads/multimedia`) ya no se usa: los binarios viven en SharepointApi.
Conviene decidir si se retira o se mantiene como alternativa sin red.
