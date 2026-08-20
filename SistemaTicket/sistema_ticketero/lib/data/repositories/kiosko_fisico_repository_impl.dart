import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';

import '../../core/errors/failures.dart';
import '../../data/datasources/remote/kiosko_fisico_remote_datasource.dart';
import '../../data/models/kiosko_fisico_model.dart';
import '../../domain/entities/kiosko_fisico.dart';
import '../../domain/repositories/kiosko_fisico_repository.dart';

class KioskoFisicoRepositoryImpl implements KioskoFisicoRepository {
  final KioskoFisicoRemoteDataSource _remote;

  KioskoFisicoRepositoryImpl(this._remote);

  @override
  Future<Either<Failure, List<KioskoFisico>>> getAll() async {
    try {
      final models = await _remote.getAll();
      return Right(models.map((m) => m.toEntity()).toList());
    } catch (e) {
      return Left(ServerFailure(message: 'Error al obtener kioskos físicos: $e'));
    }
  }

  @override
  Future<Either<Failure, KioskoFisico>> getById(int id) async {
    try {
      final model = await _remote.getById(id);
      return Right(model.toEntity());
    } catch (e) {
      return Left(ServerFailure(message: 'Error al obtener kiosko físico: $e'));
    }
  }

  Map<String, List<String>>? _parseValidationErrors(DioException e) {
    if (e.response?.statusCode == 400 && e.response?.data is Map) {
      final errors = (e.response!.data as Map)['errors'];
      if (errors is Map) {
        return errors.map((k, v) => MapEntry(k.toString(), (v as List).cast<String>()));
      }
    }
    return null;
  }

  /// Convierte una excepción en un [Failure] con el motivo real.
  ///
  /// La API responde de dos formas distintas según dónde falle: ModelState
  /// (`{errors: {...}}`) para validación de atributos, y su propio formato
  /// (`{error, mensaje, codigo}`) desde el middleware. Antes solo se entendía
  /// el primero, así que el segundo llegaba al usuario como el `toString()` de
  /// Dio, que no dice nada útil.
  Failure _mapError(Object e, String accion) {
    if (e is! DioException) return ServerFailure(message: 'Error al $accion: $e');

    final errores = _parseValidationErrors(e);
    if (errores != null) {
      return ValidationFailure(message: 'Error de validación', errors: errores);
    }

    final data = e.response?.data;
    final status = e.response?.statusCode;

    if (data is Map) {
      final mensaje = data['mensaje'] ?? data['title'] ?? data['detail'];
      if (mensaje is String && mensaje.isNotEmpty) {
        return ServerFailure(message: 'Error al $accion ($status): $mensaje', statusCode: status);
      }
    }

    if (status != null) {
      return ServerFailure(
        message: 'Error al $accion ($status): ${data ?? e.message}',
        statusCode: status,
      );
    }

    return ConnectionFailure(message: 'Sin respuesta de la API al $accion: ${e.message}');
  }

  @override
  Future<Either<Failure, KioskoFisico>> create(Map<String, dynamic> data) async {
    try {
      final model = await _remote.create(data);
      return Right(model.toEntity());
    } catch (e) {
      return Left(_mapError(e, 'crear el kiosko'));
    }
  }

  @override
  Future<Either<Failure, KioskoFisico>> update(int id, Map<String, dynamic> data) async {
    try {
      final model = await _remote.update(id, data);
      return Right(model.toEntity());
    } catch (e) {
      return Left(_mapError(e, 'actualizar el kiosko'));
    }
  }

  @override
  Future<Either<Failure, void>> delete(int id) async {
    try {
      await _remote.delete(id);
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(message: 'Error al eliminar kiosko físico: $e'));
    }
  }

  @override
  Future<Either<Failure, KioskoFisico>> autoRegistrar(Map<String, dynamic> data) async {
    try {
      final model = await _remote.autoRegistrar(data);
      return Right(KioskoFisicoModel.fromJson(model).toEntity());
    } catch (e) {
      return Left(ServerFailure(message: 'Error al auto-registrar kiosko: $e'));
    }
  }

  @override
  Future<Either<Failure, String>> detectarIp() async {
    try {
      final result = await _remote.detectarIp();
      return Right(result['ip'] as String? ?? '');
    } catch (e) {
      return Left(ServerFailure(message: 'Error al detectar IP: $e'));
    }
  }

  @override
  Future<Either<Failure, Map<String, dynamic>>> getPrinterConfig(int kioskoId) async {
    try {
      final data = await _remote.getPrinterConfig(kioskoId);
      return Right(data);
    } catch (e) {
      return Left(ServerFailure(message: 'Error al obtener configuración de impresora: $e'));
    }
  }

  @override
  Future<Either<Failure, void>> updatePrinterConfig(int kioskoId, Map<String, dynamic> data) async {
    try {
      await _remote.updatePrinterConfig(kioskoId, data);
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(message: 'Error al actualizar configuración de impresora: $e'));
    }
  }
}
