import 'package:flutter/material.dart';

class ServiceType {
  final int id;
  final int areaId;
  final String nombre;
  final String iconName;

  const ServiceType({
    required this.id,
    required this.areaId,
    required this.nombre,
    this.iconName = 'help_outline',
  });

  /// Icono del servicio.
  ///
  /// Si no se le asignó uno explícitamente, se deduce del nombre. La mayoría de
  /// los servicios se dan de alta sin elegir icono, y una pantalla de kiosko
  /// llena de interrogaciones no ayuda a nadie a decidir dónde tocar.
  IconData get icon {
    final explicito = _iconMap[iconName];
    if (explicito != null && iconName != 'help_outline') return explicito;
    return _deducirPorNombre(nombre) ?? Icons.help_outline;
  }

  static IconData? _deducirPorNombre(String nombre) {
    final n = _sinAcentos(nombre.toLowerCase());

    for (final (claves, icono) in _porPalabraClave) {
      if (claves.any(n.contains)) return icono;
    }
    return null;
  }

  /// El orden importa: gana la primera coincidencia, así que las palabras más
  /// específicas van antes que las genéricas.
  static const List<(List<String>, IconData)> _porPalabraClave = [
    (['pago', 'pagar', 'cobro', 'caja'], Icons.payments),
    (['retiro', 'retirar', 'entrega'], Icons.outbox),
    (['deposito', 'recepcion'], Icons.inbox),
    (['consulta', 'pregunta', 'duda'], Icons.live_help),
    (['informacion', 'informe'], Icons.info_outline),
    (['inscripcion', 'matricula', 'registro', 'alta'], Icons.app_registration),
    (['certificad', 'constancia'], Icons.workspace_premium),
    (['documento', 'papel'], Icons.description),
    (['recibo', 'factura', 'comprobante'], Icons.receipt_long),
    (['reclamo', 'queja', 'soporte', 'atencion'], Icons.support_agent),
    (['renovacion', 'renovar', 'actualiza'], Icons.refresh),
    (['tramite', 'solicitud'], Icons.assignment),
    (['verificacion', 'validar', 'revision'], Icons.fact_check),
    (['busqueda', 'buscar'], Icons.search),
    (['beca', 'ayuda'], Icons.volunteer_activism),
  ];

  static String _sinAcentos(String texto) {
    const con = 'áéíóúüñ';
    const sin = 'aeiouun';
    var r = texto;
    for (var i = 0; i < con.length; i++) {
      r = r.replaceAll(con[i], sin[i]);
    }
    return r;
  }

  static const Map<String, IconData> _iconMap = {
    'payments': Icons.payments,
    'search': Icons.search,
    'money_off': Icons.money_off,
    'live_help': Icons.live_help,
    'explore': Icons.explore,
    'description': Icons.description,
    'app_registration': Icons.app_registration,
    'refresh': Icons.refresh,
    'remove_circle': Icons.remove_circle,
    'inbox': Icons.inbox,
    'outbox': Icons.outbox,
    'find_in_page': Icons.find_in_page,
    'receipt': Icons.receipt,
    'assignment': Icons.assignment,
    'fact_check': Icons.fact_check,
    'support_agent': Icons.support_agent,
    'handshake': Icons.handshake,
    'help_outline': Icons.help_outline,
  };

  static const List<Map<String, dynamic>> iconOptions = [
    {'name': 'Por defecto', 'icon': 'help_outline'},
    {'name': 'Pago', 'icon': 'payments'},
    {'name': 'Búsqueda', 'icon': 'search'},
    {'name': 'Efectivo', 'icon': 'money_off'},
    {'name': 'Ayuda', 'icon': 'live_help'},
    {'name': 'Explorar', 'icon': 'explore'},
    {'name': 'Documento', 'icon': 'description'},
    {'name': 'Registro', 'icon': 'app_registration'},
    {'name': 'Renovar', 'icon': 'refresh'},
    {'name': 'Quitar', 'icon': 'remove_circle'},
    {'name': 'Entrada', 'icon': 'inbox'},
    {'name': 'Salida', 'icon': 'outbox'},
    {'name': 'Buscar', 'icon': 'find_in_page'},
    {'name': 'Recibo', 'icon': 'receipt'},
    {'name': 'Tarea', 'icon': 'assignment'},
    {'name': 'Verificar', 'icon': 'fact_check'},
    {'name': 'Atención', 'icon': 'support_agent'},
    {'name': 'Acuerdo', 'icon': 'handshake'},
  ];

  static IconData iconByName(String name) =>
      _iconMap[name] ?? Icons.help_outline;

  ServiceType copyWith({
    int? id,
    int? areaId,
    String? nombre,
    String? iconName,
  }) {
    return ServiceType(
      id: id ?? this.id,
      areaId: areaId ?? this.areaId,
      nombre: nombre ?? this.nombre,
      iconName: iconName ?? this.iconName,
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'areaId': areaId,
        'nombre': nombre,
        'iconName': iconName,
      };

  factory ServiceType.fromJson(Map<String, dynamic> json) => ServiceType(
        id: json['id'] as int,
        areaId: json['areaId'] as int,
        nombre: json['nombre'] as String,
        iconName: (json['iconName'] as String?) ?? (json['icono'] as String?) ?? 'help_outline',
      );
}
