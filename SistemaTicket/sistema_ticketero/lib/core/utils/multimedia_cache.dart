import 'dart:io';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:path_provider/path_provider.dart';

import '../constants/sharepoint_constants.dart';

/// Caché en disco de los archivos multimedia alojados en SharepointApi.
///
/// Existe por dos motivos:
///
/// 1. **`video_player_win` no envía cabeceras HTTP.** En Windows el reproductor
///    delega en Media Foundation, que abre la URL por su cuenta y no arrastra
///    `ProviderKey`, así que la descarga directa devolvería 401. Descargando
///    aquí y reproduciendo con `VideoPlayerController.file` el problema
///    desaparece.
/// 2. **Es lo correcto para un kiosko.** El video de fondo se reproduce en
///    bucle durante horas; tenerlo en disco evita re-descargarlo en cada
///    arranque y aguanta cortes de red.
class MultimediaCache {
  MultimediaCache._();

  static final Map<String, Future<File>> _enCurso = {};
  static Directory? _directorio;

  static final Dio _dio = Dio(BaseOptions(
    baseUrl: SharepointConstants.baseUrl,
    connectTimeout: const Duration(seconds: 15),
    receiveTimeout: const Duration(minutes: 5),
    responseType: ResponseType.bytes,
    headers: {
      SharepointConstants.providerKeyHeader: SharepointConstants.providerKey,
    },
  ));

  static Future<Directory> _carpeta() async {
    if (_directorio != null) return _directorio!;
    final base = await getApplicationSupportDirectory();
    final dir = Directory('${base.path}${Platform.pathSeparator}multimedia');
    if (!await dir.exists()) await dir.create(recursive: true);
    _directorio = dir;
    return dir;
  }

  /// Devuelve la ruta local de una referencia `sharepoint:{uid}`,
  /// descargándola la primera vez.
  ///
  /// Devuelve `null` si el valor no es una referencia o si la descarga falla;
  /// el llamador decide entonces qué hacer (mostrar el fondo de respaldo, etc.).
  static Future<String?> rutaLocal(String referencia) async {
    final uid = SharepointConstants.uidFromRef(referencia);
    if (uid == null) return null;

    try {
      final archivo = await _descargar(uid);
      return archivo.path;
    } catch (e) {
      debugPrint('[MultimediaCache] No se pudo obtener $uid: $e');
      return null;
    }
  }

  static Future<File> _descargar(String uid) {
    // Si dos widgets piden el mismo archivo a la vez (p. ej. el tiquetero y la
    // ventana del llamador), comparten la misma descarga.
    final enCurso = _enCurso[uid];
    if (enCurso != null) return enCurso;

    final descarga = _descargarReal(uid);
    _enCurso[uid] = descarga;
    descarga.whenComplete(() => _enCurso.remove(uid)).ignore();
    return descarga;
  }

  static Future<File> _descargarReal(String uid) async {
    final dir = await _carpeta();

    // Si ya está en disco con cualquier extensión, se reutiliza.
    final existente = dir
        .listSync()
        .whereType<File>()
        .where((f) => f.uri.pathSegments.last.startsWith(uid))
        .firstOrNull;

    if (existente != null && await existente.length() > 0) {
      return existente;
    }

    final response = await _dio.get<List<int>>(
      SharepointConstants.contenidoEndpoint(uid),
    );

    final bytes = response.data;
    if (bytes == null || bytes.isEmpty) {
      throw StateError('El servicio devolvió un archivo vacío.');
    }

    // Media Foundation elige el decodificador por la extensión, así que hay que
    // conservarla. Se toma del Content-Disposition y, si no viene, del MIME.
    final extension = _extensionDesde(response.headers) ?? '.bin';
    final destino = File('${dir.path}${Platform.pathSeparator}$uid$extension');

    // Escritura atómica: si el proceso muere a media descarga, no queda un
    // archivo truncado que luego se dé por bueno.
    final temporal = File('${destino.path}.part');
    await temporal.writeAsBytes(bytes, flush: true);
    await temporal.rename(destino.path);

    return destino;
  }

  static String? _extensionDesde(Headers headers) {
    final disposition = headers.value('content-disposition');
    if (disposition != null) {
      var nombre = RegExp(r'filename\*?=([^;]+)').firstMatch(disposition)?.group(1)?.trim();
      if (nombre != null) {
        nombre = nombre.replaceAll('"', '');
        // Forma RFC 5987: filename*=UTF-8''archivo%20final.mp4
        final separador = nombre.indexOf("''");
        if (separador >= 0) nombre = nombre.substring(separador + 2);
        if (nombre.contains('.')) {
          return nombre.substring(nombre.lastIndexOf('.')).toLowerCase();
        }
      }
    }

    final contentType = headers.value('content-type')?.split(';').first.trim();
    return switch (contentType) {
      'video/mp4' => '.mp4',
      'video/webm' => '.webm',
      'video/quicktime' => '.mov',
      'video/x-matroska' => '.mkv',
      'video/x-msvideo' => '.avi',
      'image/png' => '.png',
      'image/jpeg' => '.jpg',
      'image/webp' => '.webp',
      'image/x-icon' => '.ico',
      _ => null,
    };
  }

  /// Borra la caché. Útil si un archivo se corrompió o se reemplazó en origen.
  static Future<void> limpiar() async {
    final dir = await _carpeta();
    if (await dir.exists()) await dir.delete(recursive: true);
    _directorio = null;
  }
}
