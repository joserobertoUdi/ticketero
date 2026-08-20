import 'dart:io';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../../../core/constants/sharepoint_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../data/datasources/remote/archivo_remote_datasource.dart';
import '../../../providers/auth_provider.dart';

/// Campo de texto con subida de archivo al servicio central de gestión
/// documental.
///
/// El valor que queda en el controlador es una referencia `sharepoint:{uid}`,
/// no una ruta local: así el archivo es accesible desde cualquier kiosko.
/// El campo sigue siendo editable a mano para admitir URLs externas.
class CampoMultimedia extends StatefulWidget {
  final TextEditingController controller;
  final String label;
  final bool esVideo;

  /// Identifica al registro dueño del archivo en la auditoría de SharepointApi.
  final String referenciaOrigen;

  /// Se invoca tras una subida correcta, por si el diálogo necesita refrescarse.
  final VoidCallback? onCambio;

  const CampoMultimedia({
    super.key,
    required this.controller,
    required this.label,
    required this.esVideo,
    required this.referenciaOrigen,
    this.onCambio,
  });

  @override
  State<CampoMultimedia> createState() => _CampoMultimediaState();
}

class _CampoMultimediaState extends State<CampoMultimedia> {
  final _archivos = ArchivoRemoteDataSource();

  double? _progreso;
  bool _subiendo = false;
  String? _nombreArchivo;

  Future<void> _subir() async {
    final usuario = context.read<AuthProvider>().user?.nombreUsuario ?? 'desconocido';

    final result = await FilePicker.pickFiles(
      type: FileType.custom,
      allowedExtensions: widget.esVideo
          ? SharepointConstants.videoExtensions
          : SharepointConstants.imageExtensions,
      allowMultiple: false,
      dialogTitle: 'Seleccionar ${widget.esVideo ? 'video' : 'imagen'}',
    );

    final path = result?.files.single.path;
    if (path == null || !mounted) return;

    setState(() {
      _subiendo = true;
      _progreso = 0;
    });

    try {
      final subido = await _archivos.subirArchivo(
        archivo: File(path),
        usuarioRegistro: usuario,
        referenciaOrigen: widget.referenciaOrigen,
        onProgress: (p) {
          if (mounted) setState(() => _progreso = p);
        },
      );

      if (!mounted) return;
      widget.controller.text = subido.referencia;
      setState(() => _nombreArchivo = subido.nombreArchivo);
      widget.onCambio?.call();
      _aviso('"${subido.nombreArchivo}" subido correctamente.');
    } on ArchivoException catch (e) {
      _aviso(e.message, esError: true);
    } catch (e) {
      _aviso('Error inesperado al subir: $e', esError: true);
    } finally {
      if (mounted) {
        setState(() {
          _subiendo = false;
          _progreso = null;
        });
      }
    }
  }

  void _aviso(String mensaje, {bool esError = false}) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(
      content: Text(mensaje),
      backgroundColor: esError ? AppColors.error : AppColors.success,
      duration: Duration(seconds: esError ? 8 : 3),
    ));
  }

  @override
  Widget build(BuildContext context) {
    final valor = widget.controller.text.trim();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: TextField(
                controller: widget.controller,
                decoration: InputDecoration(
                  labelText: widget.label,
                  hintText: widget.esVideo
                      ? 'Sube un archivo o pega una URL directa a .mp4'
                      : 'Sube un archivo o pega una URL directa a la imagen',
                  border: const OutlineInputBorder(),
                  isDense: true,
                ),
                style: const TextStyle(fontSize: 13),
                onChanged: (_) => setState(() => _nombreArchivo = null),
              ),
            ),
            const SizedBox(width: 8),
            SizedBox(
              height: 48,
              child: ElevatedButton.icon(
                onPressed: _subiendo ? null : _subir,
                icon: const Icon(Icons.cloud_upload_outlined, size: 18),
                label: Text(_subiendo ? 'Subiendo…' : 'Subir'),
              ),
            ),
            if (valor.isNotEmpty) ...[
              const SizedBox(width: 4),
              SizedBox(
                height: 48,
                child: IconButton(
                  tooltip: 'Quitar',
                  icon: const Icon(Icons.close, size: 18, color: AppColors.error),
                  onPressed: _subiendo
                      ? null
                      : () {
                          widget.controller.clear();
                          setState(() => _nombreArchivo = null);
                          widget.onCambio?.call();
                        },
                ),
              ),
            ],
          ],
        ),
        if (_subiendo)
          Padding(
            padding: const EdgeInsets.only(top: 6),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                LinearProgressIndicator(value: _progreso),
                const SizedBox(height: 4),
                Text(
                  'Subiendo al servicio central… '
                  '${_progreso != null ? '${(_progreso! * 100).toStringAsFixed(0)}%' : ''}',
                  style: const TextStyle(fontSize: 11, color: AppColors.textSecondary),
                ),
              ],
            ),
          )
        else
          _origen(valor),
      ],
    );
  }

  /// Deja claro si el archivo es accesible desde otros equipos. Una ruta local
  /// solo funciona en la máquina donde se configuró, y ese fallo es silencioso.
  Widget _origen(String valor) {
    if (valor.isEmpty) return const SizedBox(height: 6);

    final (IconData icono, Color color, String texto) = switch (valor) {
      _ when SharepointConstants.isRef(valor) => (
          Icons.cloud_done_outlined,
          AppColors.success,
          'En el servicio central'
              '${_nombreArchivo != null ? ' · $_nombreArchivo' : ''}'
        ),
      _ when valor.startsWith('http') && _pareceReproducible(valor) => (
          Icons.link,
          AppColors.textSecondary,
          'URL externa'
        ),
      _ when valor.startsWith('http') => (
          Icons.error_outline,
          AppColors.error,
          widget.esVideo
              ? 'Esta URL no apunta a un archivo de video. YouTube y similares '
                  'no se pueden reproducir: hace falta un enlace directo a .mp4'
              : 'Esta URL no parece apuntar a una imagen'
        ),
      _ => (
          Icons.warning_amber_outlined,
          Colors.orange,
          'Ruta local: solo funciona en este equipo'
        ),
    };

    return Padding(
      padding: const EdgeInsets.only(top: 6),
      child: Row(
        children: [
          Icon(icono, size: 14, color: color),
          const SizedBox(width: 6),
          Expanded(
            child: Text(texto, style: TextStyle(fontSize: 11, color: color)),
          ),
        ],
      ),
    );
  }

  bool _pareceReproducible(String url) {
    final ruta = Uri.tryParse(url)?.path.toLowerCase() ?? '';
    final extensiones = widget.esVideo
        ? SharepointConstants.videoExtensions
        : SharepointConstants.imageExtensions;
    return extensiones.any((e) => ruta.endsWith('.$e'));
  }
}
