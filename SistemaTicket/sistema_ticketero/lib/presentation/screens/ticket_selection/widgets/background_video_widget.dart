import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:video_player/video_player.dart';

import '../../../../core/constants/sharepoint_constants.dart';
import '../../../../core/utils/multimedia_cache.dart';

class BackgroundVideoWidget extends StatefulWidget {
  final String? videoUrl;

  const BackgroundVideoWidget({super.key, this.videoUrl});

  @override
  State<BackgroundVideoWidget> createState() => _BackgroundVideoWidgetState();
}

class _BackgroundVideoWidgetState extends State<BackgroundVideoWidget> {
  VideoPlayerController? _controller;
  bool _videoError = false;

  @override
  void initState() {
    super.initState();
    _initVideo();
  }

  @override
  void didUpdateWidget(covariant BackgroundVideoWidget oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.videoUrl != widget.videoUrl) {
      _disposeVideo();
      _videoError = false;
      _initVideo();
    }
  }

  Future<void> _initVideo() async {
    final url = widget.videoUrl;
    if (url == null || url.isEmpty) {
      debugPrint('[BackgroundVideo] Sin videoUrl: el kiosko seleccionado no '
          'tiene video configurado, o no se ha refrescado desde la API.');
      _videoError = true;
      return;
    }
    debugPrint('[BackgroundVideo] videoUrl recibido: "$url"');

    try {
      final VideoPlayerController controller;

      if (SharepointConstants.isRef(url)) {
        // Se descarga a disco en vez de reproducir por red: video_player_win
        // delega en Media Foundation, que abre la URL por su cuenta y no envía
        // la cabecera ProviderKey, con lo que recibiría un 401.
        final ruta = await MultimediaCache.rutaLocal(url);
        if (!mounted) return;
        if (ruta == null) {
          debugPrint('[BackgroundVideo] No se pudo descargar $url');
          setState(() => _videoError = true);
          return;
        }
        controller = VideoPlayerController.file(File(ruta));
      } else if (url.startsWith('http://') || url.startsWith('https://')) {
        controller = VideoPlayerController.networkUrl(Uri.parse(url));
      } else {
        controller = VideoPlayerController.file(File(url));
      }

      _controller = controller;
      await controller.initialize();

      if (!mounted) {
        _disposeVideo();
        return;
      }

      await controller.setLooping(true);
      await controller.play();
      setState(() => _videoError = false);
    } catch (e, stack) {
      // Antes esto se tragaba en silencio y la pantalla quedaba en blanco sin
      // ninguna pista de por qué.
      debugPrint('[BackgroundVideo] Fallo al reproducir "${widget.videoUrl}": $e');
      debugPrintStack(stackTrace: stack, maxFrames: 6);
      _disposeVideo();
      if (mounted) setState(() => _videoError = true);
    }
  }

  void _disposeVideo() {
    _controller?.dispose();
    _controller = null;
  }

  @override
  void dispose() {
    _disposeVideo();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (_controller != null && _controller!.value.isInitialized && !_videoError) {
      return SizedBox.expand(
        child: FittedBox(
          fit: BoxFit.cover,
          child: SizedBox(
            width: _controller!.value.size.width,
            height: _controller!.value.size.height,
            child: VideoPlayer(_controller!),
          ),
        ),
      );
    }

    if (_videoError) {
      return const SizedBox.expand(child: ColoredBox(color: Colors.white));
    }

    return Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [
            Color(0xFFB00510),
            Color(0xFFE30613),
          ],
        ),
      ),
      child: Stack(
        children: [
          Positioned.fill(
            child: CustomPaint(
              painter: _PatternPainter(),
            ),
          ),
          Container(
            color: Colors.black.withValues(alpha: 0.15),
          ),
        ],
      ),
    );
  }
}

class _PatternPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = Colors.white.withValues(alpha: 0.03)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1;

    const spacing = 60.0;
    for (double x = 0; x < size.width; x += spacing) {
      canvas.drawLine(Offset(x, 0), Offset(x, size.height), paint);
    }
    for (double y = 0; y < size.height; y += spacing) {
      canvas.drawLine(Offset(0, y), Offset(size.width, y), paint);
    }
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
