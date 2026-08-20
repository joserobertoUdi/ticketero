/// Configuración del servicio central de gestión documental (SharepointApi).
///
/// Guarda y recupera los archivos multimedia de los kioskos (logos y videos de
/// fondo) para que sean accesibles desde cualquier máquina, en lugar de
/// depender de rutas locales.
class SharepointConstants {
  SharepointConstants._();

  static const String baseUrl = 'http://10.1.210.10/SharepointApi';

  /// Cabecera de autenticación que espera el servicio.
  static const String providerKeyHeader = 'ProviderKey';
  static const String providerKey =
      r'6F7vP@9xL2#qMz8!sKd4^Yw1&Hr5*Nb3%Tc7Ju0Ai9Xe2Vp6GmQ8Rn1Cs4Ef5Zh';

  /// Contenedor lógico destino. Debe existir en la tabla `Contenedores` de la
  /// base de SharepointApi con `Activo = 1`; si no, el servicio responde 400
  /// con `ContenedorNoConfiguradoException`.
  ///
  /// Sus columnas `ExtensionesPermitidas` y `TamanoMaximoMb` mandan sobre lo
  /// que se declara aquí abajo.
  static const String contenedor = 'Ticketero';

  /// Identifica al sistema que origina el archivo.
  static const String entidadOrigen = 'KioskoMultimedia';

  // Endpoints
  static const String uploadEndpoint = '/api/Archivos';
  static const String uploadBase64Endpoint = '/api/Archivos/base64';

  static String metadataEndpoint(String uid) => '/api/Archivos/$uid';
  static String contenidoEndpoint(String uid) => '/api/Archivos/$uid/contenido';
  static String base64Endpoint(String uid) => '/api/Archivos/$uid/base64';

  /// URL absoluta para descargar el binario de un archivo.
  ///
  /// Requiere la cabecera [providerKeyHeader]; usar junto con [downloadHeaders].
  static String contenidoUrl(String uid) => '$baseUrl${contenidoEndpoint(uid)}';

  static const Map<String, String> downloadHeaders = {
    providerKeyHeader: providerKey,
  };

  /// Prefijo con el que se marcan las referencias guardadas en
  /// `KioskoMedia.logoUrl` / `videoUrl`.
  ///
  /// Se guarda `sharepoint:{uid}` en lugar de la URL completa para que un
  /// cambio de host o de ruta del servicio no invalide la configuración de
  /// todos los kioskos.
  static const String refPrefix = 'sharepoint:';

  static String buildRef(String uid) => '$refPrefix$uid';

  static bool isRef(String value) => value.startsWith(refPrefix);

  static String? uidFromRef(String value) =>
      isRef(value) ? value.substring(refPrefix.length) : null;

  /// Traduce una referencia guardada a una URL descargable.
  /// Devuelve el valor original si no es una referencia de SharepointApi.
  static String resolveUrl(String value) {
    final uid = uidFromRef(value);
    return uid != null ? contenidoUrl(uid) : value;
  }

  /// Extensiones aceptadas por el panel de multimedia.
  ///
  /// Nota: `PoliticaExtensiones` del servicio bloquea `.svg` por riesgo de XSS
  /// almacenado, por eso no aparece entre las imágenes. Además, cada contenedor
  /// puede tener su propia lista blanca, más restrictiva que esta.
  static const List<String> imageExtensions = ['png', 'jpg', 'jpeg', 'ico', 'webp'];
  static const List<String> videoExtensions = ['mp4', 'avi', 'mov', 'mkv', 'webm'];

  /// Tope global de la petición en SharepointApi
  /// (`Limites:TamanoMaximoPeticionMb`, por defecto 60 MB).
  ///
  /// El contenedor puede imponer un tope menor mediante `TamanoMaximoMb`; en ese
  /// caso el servicio devuelve 400 con el detalle, que se muestra tal cual.
  static const int maxFileSizeBytes = 60 * 1024 * 1024;
}
