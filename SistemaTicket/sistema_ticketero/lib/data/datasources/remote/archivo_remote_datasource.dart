import 'dart:io';

import 'package:dio/dio.dart';

import '../../../core/constants/sharepoint_constants.dart';

/// Resultado de subir un archivo a SharepointApi.
class ArchivoSubido {
  /// Identificador que devuelve el servicio; con él se recupera el contenido.
  final String uid;

  /// Referencia que se persiste en la configuración del kiosko
  /// (`sharepoint:{uid}`).
  final String referencia;

  /// Nombre original del archivo, para mostrarlo en la interfaz.
  final String nombreArchivo;

  const ArchivoSubido({
    required this.uid,
    required this.referencia,
    required this.nombreArchivo,
  });
}

/// Excepción con un mensaje ya redactado para mostrar al usuario.
class ArchivoException implements Exception {
  final String message;
  const ArchivoException(this.message);

  @override
  String toString() => message;
}

/// Cliente del servicio central de gestión documental.
///
/// Usa su propia instancia de [Dio] porque apunta a un host distinto al de la
/// API de Ticketero y se autentica con una cabecera propia en vez de JWT.
class ArchivoRemoteDataSource {
  final Dio _dio;

  ArchivoRemoteDataSource({Dio? dio})
      : _dio = dio ??
            Dio(BaseOptions(
              baseUrl: SharepointConstants.baseUrl,
              connectTimeout: const Duration(seconds: 15),
              // Un video puede tardar; no aplicamos receiveTimeout agresivo.
              sendTimeout: const Duration(minutes: 10),
              receiveTimeout: const Duration(minutes: 2),
              headers: {
                SharepointConstants.providerKeyHeader:
                    SharepointConstants.providerKey,
              },
            ));

  /// Sube un archivo mediante `POST /api/Archivos` (multipart/form-data).
  ///
  /// [onProgress] recibe un valor entre 0 y 1, o `null` si el servidor no
  /// informa del total.
  Future<ArchivoSubido> subirArchivo({
    required File archivo,
    required String usuarioRegistro,
    String? referenciaOrigen,
    void Function(double? progreso)? onProgress,
  }) async {
    if (!await archivo.exists()) {
      throw const ArchivoException('El archivo seleccionado ya no existe.');
    }

    final tamano = await archivo.length();
    if (tamano == 0) {
      throw const ArchivoException('El archivo está vacío.');
    }
    if (tamano > SharepointConstants.maxFileSizeBytes) {
      final mb = (tamano / (1024 * 1024)).toStringAsFixed(1);
      throw ArchivoException(
          'El archivo pesa $mb MB y supera el límite de '
          '${SharepointConstants.maxFileSizeBytes ~/ (1024 * 1024)} MB.');
    }

    final nombreArchivo = archivo.uri.pathSegments.last;

    final formData = FormData.fromMap({
      'Contenedor': SharepointConstants.contenedor,
      'Archivo': await MultipartFile.fromFile(archivo.path, filename: nombreArchivo),
      'EntidadOrigen': SharepointConstants.entidadOrigen,
      if (referenciaOrigen != null) 'ReferenciaOrigen': referenciaOrigen,
      'UsuarioRegistro': usuarioRegistro,
    });

    try {
      final response = await _dio.post(
        SharepointConstants.uploadEndpoint,
        data: formData,
        onSendProgress: (enviado, total) {
          if (onProgress == null) return;
          onProgress(total > 0 ? enviado / total : null);
        },
      );

      // Contrato de SharepointApi (ApiSuccessResponse<ArchivoDto>):
      // { exito, mensaje, detalles, datos: { uid, nombreArchivo, contentType, ... } }
      final datos = _datos(response.data);
      if (datos == null) {
        throw ArchivoException(
            'Respuesta inesperada de SharepointApi: ${response.data}');
      }

      final uid = datos['uid'] as String?;
      if (uid == null || uid.isEmpty) {
        throw ArchivoException(
            'El servicio no devolvió el uid del archivo. Respuesta: ${response.data}');
      }

      return ArchivoSubido(
        uid: uid,
        referencia: SharepointConstants.buildRef(uid),
        nombreArchivo: (datos['nombreArchivo'] as String?) ?? nombreArchivo,
      );
    } on DioException catch (e) {
      throw ArchivoException(_mensajeError(e));
    }
  }

  /// Comprueba que un archivo sigue existiendo en el servicio.
  Future<bool> existe(String uid) async {
    try {
      final r = await _dio.get(SharepointConstants.metadataEndpoint(uid));
      return r.statusCode == 200;
    } on DioException {
      return false;
    }
  }

  /// Extrae el objeto `datos` del envoltorio `ApiSuccessResponse<T>`.
  Map<String, dynamic>? _datos(dynamic body) {
    if (body is! Map) return null;
    final datos = body['datos'];
    return datos is Map ? datos.cast<String, dynamic>() : null;
  }

  /// El middleware de SharepointApi responde siempre con `ApiErrorResponse`,
  /// que trae el motivo real en `mensaje`. Sin esto solo veríamos el código HTTP.
  String? _mensajeServidor(dynamic body) {
    if (body is Map && body['mensaje'] is String) return body['mensaje'] as String;
    return null;
  }

  String _mensajeError(DioException e) {
    final status = e.response?.statusCode;
    final delServidor = _mensajeServidor(e.response?.data);

    // 400 lo devuelven ContenedorNoConfiguradoException y ArchivoInvalidoException
    // (extensión bloqueada, contenedor inexistente, archivo por encima del tope
    // del contenedor). El mensaje del servidor ya explica cuál fue.
    if (delServidor != null) {
      return switch (status) {
        401 => 'SharepointApi rechazó la ProviderKey: $delServidor',
        400 => delServidor,
        404 => 'Archivo no encontrado: $delServidor',
        429 => 'Demasiadas subidas seguidas; espera un minuto.',
        502 => 'SharepointApi no pudo guardar en SharePoint: $delServidor',
        _ => 'SharepointApi respondió $status: $delServidor',
      };
    }

    if (status == 413) {
      return 'El archivo supera el tamaño máximo de petición del servidor '
          '(${SharepointConstants.maxFileSizeBytes ~/ (1024 * 1024)} MB).';
    }
    if (status != null) {
      return 'SharepointApi respondió $status: ${e.response?.data ?? e.message}';
    }

    switch (e.type) {
      case DioExceptionType.connectionError:
      case DioExceptionType.connectionTimeout:
        return 'No se pudo conectar con SharepointApi '
            '(${SharepointConstants.baseUrl}). Revisa la red del kiosko.';
      case DioExceptionType.sendTimeout:
        return 'La subida superó el tiempo máximo. Prueba con un archivo más ligero.';
      case DioExceptionType.receiveTimeout:
        return 'SharepointApi no respondió a tiempo.';
      default:
        return 'Error al subir el archivo: ${e.message ?? e.type.name}';
    }
  }
}
