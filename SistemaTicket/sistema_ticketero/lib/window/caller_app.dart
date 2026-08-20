import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:provider/provider.dart';
import 'package:window_manager/window_manager.dart';

import '../core/network/api_client.dart';
import '../core/theme/app_theme.dart';
import '../core/utils/time_sync_service.dart';
import '../data/datasources/local/auth_local_datasource.dart';
import '../data/datasources/local/ticket_local_datasource.dart';
import '../data/datasources/remote/auth_remote_datasource.dart';
import '../data/datasources/remote/kiosko_fisico_remote_datasource.dart';
import '../data/datasources/remote/ticket_remote_datasource.dart';
import '../data/repositories/auth_repository_impl.dart';
import '../data/repositories/kiosko_fisico_repository_impl.dart';
import '../data/repositories/ticket_repository_impl.dart';
import '../presentation/providers/auth_provider.dart';
import '../presentation/providers/kiosko_fisico_provider.dart';
import '../presentation/providers/ticket_provider.dart';
import '../presentation/providers/settings_provider.dart';
import '../presentation/screens/caller/caller_screen.dart';
import 'ticket_channel.dart';

class CallerWindowApp extends StatefulWidget {
  const CallerWindowApp({super.key});

  @override
  State<CallerWindowApp> createState() => _CallerWindowAppState();
}

class _CallerWindowAppState extends State<CallerWindowApp>
    with WindowListener {
  @override
  void initState() {
    super.initState();
    windowManager.addListener(this);
    _initWindow();
  }

  Future<void> _initWindow() async {
    await windowManager.show();
    await windowManager.focus();
    // Inicializar sincronización horaria con el servidor
    final timeSync = TimeSyncService();
    unawaited(timeSync.initialize());
  }

  @override
  void dispose() {
    windowManager.removeListener(this);
    super.dispose();
  }

  @override
  void onWindowClose() async {
    await windowManager.destroy();
  }

  @override
  Widget build(BuildContext context) {
    final apiClient = ApiClient();
    final timeSyncService = TimeSyncService();
    return MaterialApp(
      title: 'Sistema Ticketero - Llamador',
      theme: AppTheme.lightTheme,
      debugShowCheckedModeBanner: false,
      home: MultiProvider(
        providers: [
          Provider<ApiClient>.value(value: apiClient),
          Provider<TimeSyncService>.value(value: timeSyncService),
          ChangeNotifierProvider(create: (_) => AuthProvider(
            authRepository: AuthRepositoryImpl(
              AuthRemoteDataSource(apiClient),
              AuthLocalDataSource(const FlutterSecureStorage()),
              apiClient,
            ),
          )),
          ChangeNotifierProvider(create: (_) => TicketProvider(
            ticketRepository: TicketRepositoryImpl(
              TicketRemoteDataSource(apiClient),
              TicketLocalDataSource(),
            ),
            timeSync: timeSyncService,
          )),
          ChangeNotifierProvider(create: (_) => SettingsProvider()),
          ChangeNotifierProvider(create: (_) => KioskoFisicoProvider(
            KioskoFisicoRepositoryImpl(
              KioskoFisicoRemoteDataSource(apiClient),
            ),
          )),
        ],
        child: const CallerWindowScreen(),
      ),
    );
  }
}

class CallerWindowScreen extends StatefulWidget {
  const CallerWindowScreen({super.key});

  @override
  State<CallerWindowScreen> createState() => _CallerWindowScreenState();
}

class _CallerWindowScreenState extends State<CallerWindowScreen> with WindowListener {
  DateTime? _ultimaCarga;

  @override
  void initState() {
    super.initState();
    windowManager.addListener(this);
    _listenChannel();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadSettings();
    });
  }

  @override
  void dispose() {
    windowManager.removeListener(this);
    super.dispose();
  }

  /// Esta ventana no se cierra entre sesiones: el login la reutiliza con
  /// `show()` si ya existe, y entonces `initState` no vuelve a ejecutarse. Sin
  /// recargar al recibir el foco, se quedaba con la configuración que tenía el
  /// día que se abrió.
  @override
  void onWindowFocus() {
    final ahora = DateTime.now();
    if (_ultimaCarga != null && ahora.difference(_ultimaCarga!).inSeconds < 5) {
      return;
    }
    _loadSettings();
  }

  Future<void> _loadSettings() async {
    _ultimaCarga = DateTime.now();

    final sp = Provider.of<SettingsProvider>(context, listen: false);
    final kioskoProvider = Provider.of<KioskoFisicoProvider>(context, listen: false);
    final apiClient = Provider.of<ApiClient>(context, listen: false);
    await sp.loadSettings();

    // Esta ventana es un motor Flutter aparte y construye su propio ApiClient,
    // que nace sin token. Sin esto, /api/kioskos-fisicos responde 401 y la
    // lista llega vacía, sin error visible.
    final token = await AuthLocalDataSource(const FlutterSecureStorage()).getToken();
    if (token == null || token.isEmpty) {
      debugPrint('[Caller] Sin token guardado: no se puede consultar la API.');
      return;
    }
    apiClient.setToken(token);

    try {
      await kioskoProvider.loadAll();
      final activos = kioskoProvider.kioskosActivos;
      if (activos.isEmpty) {
        debugPrint('[Caller] La API no devolvió kioskos activos '
            '(${kioskoProvider.kioskos.length} en total).');
        return;
      }

      // El kiosko elegido manda; `first` solo como respaldo, porque con varios
      // kioskos activos elegiría uno al azar.
      final k = activos.where((x) => x.id == sp.selectedKioskoId).firstOrNull ??
          activos.first;

      await sp.setSelectedKioskoId(
        k.id,
        areaIds: k.areaIds,
        logoUrl: k.logoUrl,
        videoUrl: k.videoUrl,
        nombre: k.nombre,
      );
      debugPrint('[Caller] Kiosko ${k.id} cargado. video="${k.videoUrl ?? ''}"');
    } catch (e) {
      // Sin API se sigue con lo que ya había en SharedPreferences.
      debugPrint('[Caller] No se pudo refrescar el kiosko: $e');
    }
  }

  void _listenChannel() {
    CallerChannel.setHandler((state) {
      final tp = Provider.of<TicketProvider>(context, listen: false);
      tp.updateFromChannel(state);
    });
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) async {
        if (!didPop) {
          await windowManager.close();
        }
      },
      child: const CallerScreen(),
    );
  }
}
