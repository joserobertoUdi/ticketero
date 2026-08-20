import '../../../core/constants/api_constants.dart';
import '../../../core/network/api_client.dart';

/// Registro de multimedia de kiosko en la API de Ticketero.
///
/// El binario vive en SharepointApi; aquí solo se guarda la referencia
/// (`sharepoint:{uid}`) para que todos los kioskos vean la misma configuración.
class MultimediaRemoteDataSource {
  final ApiClient _client;

  MultimediaRemoteDataSource(this._client);

  static const String _base = '${ApiConstants.apiPrefix}/ConfiguracionMultimedia';

  /// Tipos que reconoce `GET /api/ConfiguracionMultimedia/kiosko/{id}/config`.
  static const String tipoVideo = 'Video';
  static const String tipoLogo = 'Logo';

  Future<List<Map<String, dynamic>>> obtenerPorKiosko(int kioskoId) async {
    final response = await _client.get(_base, queryParams: {'kioskoId': kioskoId});
    final data = response.data;
    if (data is List) {
      return data.cast<Map<String, dynamic>>();
    }
    return const [];
  }

  Future<Map<String, dynamic>> crear({
    required int kioskoId,
    required String tipoContenido,
    required String nombreContenido,
    required String referencia,
    int orden = 1,
    bool repetir = true,
  }) async {
    final response = await _client.post(_base, data: {
      'kioskoId': kioskoId,
      'tipoContenido': tipoContenido,
      'nombreContenido': nombreContenido,
      'rutaArchivo': referencia,
      'orden': orden,
      'repetir': repetir,
      'estado': true,
    });
    return (response.data as Map).cast<String, dynamic>();
  }

  Future<void> eliminar(int id) async {
    await _client.delete('$_base/$id');
  }

  /// Sustituye el multimedia de un tipo para un kiosko: da de baja lo anterior
  /// y registra la referencia nueva.
  Future<void> reemplazar({
    required int kioskoId,
    required String tipoContenido,
    required String nombreContenido,
    required String referencia,
  }) async {
    final existentes = await obtenerPorKiosko(kioskoId);
    for (final item in existentes) {
      if (item['tipoContenido'] == tipoContenido && item['id'] is int) {
        await eliminar(item['id'] as int);
      }
    }
    await crear(
      kioskoId: kioskoId,
      tipoContenido: tipoContenido,
      nombreContenido: nombreContenido,
      referencia: referencia,
    );
  }
}
