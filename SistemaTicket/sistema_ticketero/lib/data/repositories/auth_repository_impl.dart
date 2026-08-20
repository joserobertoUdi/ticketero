import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import '../../core/enums/user_role.dart';
import '../../core/errors/failures.dart';
import '../../core/network/api_client.dart';
import '../../data/datasources/local/auth_local_datasource.dart';
import '../../data/datasources/remote/auth_remote_datasource.dart';
import '../../domain/entities/user.dart';
import '../../domain/repositories/auth_repository.dart';

class AuthRepositoryImpl implements AuthRepository {
  final AuthRemoteDataSource _remote;
  final AuthLocalDataSource _local;
  final ApiClient _client;

  AuthRepositoryImpl(this._remote, this._local, this._client);

  @override
  Future<Either<Failure, AuthResult>> login(
      String correo, String password) async {
    try {
      final response = await _remote.login(correo, password);
      await _local.saveToken(response.token);
      await _local.saveRefreshToken(response.refreshToken);
      await _local.saveUser({
        'id': response.usuario.id,
        'nombreUsuario': response.usuario.nombreUsuario,
        'nombreCompleto': response.usuario.nombreCompleto,
        'email': response.usuario.email,
        'rol': response.usuario.rol,
        'areaId': response.usuario.areaId,
        'areaNombre': response.usuario.areaNombre,
        'areasAtencion': response.usuario.areasAtencion,
        'activo': response.usuario.activo,
        'ultimoAcceso': response.usuario.ultimoAcceso?.toIso8601String(),
      });
      _client.setToken(response.token);
      return Right(response.toAuthResult());
    } catch (e) {
      return Left(_mapError(e, contexto: 'iniciar sesión'));
    }
  }

  /// Traduce una excepción a un [Failure] concreto.
  ///
  /// Antes todo caía en un mensaje único de "credenciales inválidas", lo que
  /// hacía indistinguible un 401 real de un fallo del servidor o de red.
  Failure _mapError(Object e, {required String contexto}) {
    if (e is DioException) {
      final status = e.response?.statusCode;

      if (status == 401) {
        return const AuthFailure(
            message: 'Correo o contraseña incorrectos', statusCode: 401);
      }

      if (status != null) {
        return ServerFailure(
          message: 'El servidor respondió $status: ${_mensajeServidor(e.response?.data) ?? 'error al $contexto'}',
          statusCode: status,
        );
      }

      switch (e.type) {
        case DioExceptionType.connectionError:
        case DioExceptionType.connectionTimeout:
          return const ConnectionFailure(
              message: 'No se pudo conectar con la API. ¿Está levantada en localhost:5000?');
        case DioExceptionType.receiveTimeout:
        case DioExceptionType.sendTimeout:
          return const ConnectionFailure(
              message: 'La API no respondió a tiempo.');
        default:
          return ConnectionFailure(message: 'Error de red al $contexto: ${e.message ?? e.type.name}');
      }
    }

    // Fallos locales: almacenamiento seguro, parseo del JSON, etc.
    return ServerFailure(message: 'Error al $contexto: $e');
  }

  /// Extrae el campo `mensaje` que devuelven los controladores y el
  /// middleware de errores de la API.
  String? _mensajeServidor(dynamic data) {
    if (data is Map && data['mensaje'] is String) {
      final mensaje = data['mensaje'] as String;
      return mensaje.length > 200 ? '${mensaje.substring(0, 200)}…' : mensaje;
    }
    return null;
  }

  @override
  Future<Either<Failure, AuthResult>> refreshToken(String refreshToken) async {
    try {
      final data = await _remote.refreshToken(refreshToken);
      final newToken = data['token'] as String;
      final newRefresh = data['refreshToken'] as String;
      await _local.saveToken(newToken);
      await _local.saveRefreshToken(newRefresh);
      _client.setToken(newToken);

      final userData = await _local.getUser();
      if (userData == null) {
        return Left(ServerFailure(message: 'Sesion expirada'));
      }

      return Right(AuthResult(
        user: User(
          id: userData['id'] as int? ?? 0,
          nombreUsuario: userData['nombreUsuario'] as String? ?? '',
          nombreCompleto: userData['nombreCompleto'] as String? ?? '',
          email: userData['email'] as String? ?? '',
          rol: UserRole.fromApiValue(userData['rol'] as String? ?? 'operador'),
          areaId: userData['areaId'] as int?,
          areaNombre: userData['areaNombre'] as String?,
          areasAtencion: (userData['areasAtencion'] as List<dynamic>?)
                  ?.map((e) => e as int)
                  .toList() ??
              [],
          activo: userData['activo'] as bool? ?? true,
          ultimoAcceso: userData['ultimoAcceso'] != null
              ? DateTime.tryParse(userData['ultimoAcceso'] as String)
              : null,
        ),
        token: newToken,
        refreshToken: newRefresh,
        expiresAt: DateTime.now().add(const Duration(hours: 8)),
      ));
    } catch (e) {
      return Left(_mapError(e, contexto: 'refrescar el token'));
    }
  }

  @override
  Future<void> logout() async {
    _client.setToken(null);
    await _local.clear();
  }

  @override
  Future<Either<Failure, int>> openSession(int usuarioId, int puestoId) async {
    try {
      final data = await _remote.openSession(usuarioId, puestoId);
      final sesionId = data['sesionOperadorId'] as int;
      return Right(sesionId);
    } catch (e) {
      return Left(_mapError(e, contexto: 'abrir la sesión'));
    }
  }

  @override
  Future<Either<Failure, Unit>> closeSession(int sesionOperadorId) async {
    try {
      await _remote.closeSession(sesionOperadorId);
      return Right(unit);
    } catch (e) {
      return Left(_mapError(e, contexto: 'cerrar la sesión'));
    }
  }

  @override
  Future<Either<Failure, bool>> isAuthenticated() async {
    try {
      final token = await _local.getToken();
      return Right(token != null && token.isNotEmpty);
    } catch (e) {
      return const Right(false);
    }
  }
}
