import 'dart:ui';

import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';

/// Tarjeta de servicio del kiosko.
///
/// Está pensada para una pantalla táctil vista de pie y a distancia, sobre un
/// video de fondo que puede ser claro u oscuro. De ahí las tres decisiones
/// principales: cristal esmerilado para garantizar contraste sobre cualquier
/// fotograma, área táctil grande, y una reacción visible al tocar — sin ella
/// el usuario duda y vuelve a pulsar.
class ServiceTypeCard extends StatefulWidget {
  final String label;
  final IconData icon;
  final bool isSelected;
  final VoidCallback onTap;

  const ServiceTypeCard({
    super.key,
    required this.label,
    required this.icon,
    required this.isSelected,
    required this.onTap,
  });

  @override
  State<ServiceTypeCard> createState() => _ServiceTypeCardState();
}

class _ServiceTypeCardState extends State<ServiceTypeCard> {
  bool _presionada = false;

  static const _radio = 22.0;
  static const _duracion = Duration(milliseconds: 160);

  @override
  Widget build(BuildContext context) {
    final activa = widget.isSelected;

    return GestureDetector(
      onTapDown: (_) => setState(() => _presionada = true),
      onTapUp: (_) => setState(() => _presionada = false),
      onTapCancel: () => setState(() => _presionada = false),
      onTap: widget.onTap,
      child: AnimatedScale(
        scale: _presionada ? 0.96 : 1,
        duration: _duracion,
        curve: Curves.easeOut,
        child: AnimatedContainer(
          duration: _duracion,
          curve: Curves.easeOut,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(_radio),
            boxShadow: [
              BoxShadow(
                color: activa
                    ? AppColors.primary.withValues(alpha: 0.28)
                    : Colors.black.withValues(alpha: 0.16),
                blurRadius: activa ? 26 : 16,
                offset: Offset(0, activa ? 10 : 6),
                spreadRadius: activa ? -2 : -4,
              ),
            ],
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(_radio),
            child: BackdropFilter(
              filter: ImageFilter.blur(sigmaX: 18, sigmaY: 18),
              child: AnimatedContainer(
                duration: _duracion,
                curve: Curves.easeOut,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(_radio),
                  border: Border.all(
                    color: activa
                        ? AppColors.primary
                        : Colors.white.withValues(alpha: 0.65),
                    width: activa ? 2.5 : 1.2,
                  ),
                  // Degradado sutil: el borde superior queda algo más claro,
                  // lo que da sensación de relieve sin recurrir a sombras
                  // internas, que sobre video se ven sucias.
                  gradient: LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: activa
                        ? [
                            Colors.white.withValues(alpha: 0.95),
                            const Color(0xFFFFF3F4).withValues(alpha: 0.92),
                          ]
                        : [
                            Colors.white.withValues(alpha: 0.88),
                            Colors.white.withValues(alpha: 0.78),
                          ],
                  ),
                ),
                padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 16),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    _buildIcono(activa),
                    const SizedBox(height: 18),
                    Flexible(
                      child: AnimatedDefaultTextStyle(
                        duration: _duracion,
                        style: TextStyle(
                          fontSize: 17,
                          height: 1.2,
                          fontWeight: FontWeight.w700,
                          letterSpacing: 0.4,
                          color: activa ? AppColors.primary : const Color(0xFF2A2A2A),
                        ),
                        child: Text(
                          widget.label.toUpperCase(),
                          textAlign: TextAlign.center,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ),
                    const SizedBox(height: 10),
                    // Subrayado que crece al seleccionar: confirma la elección
                    // sin añadir texto que haya que leer.
                    AnimatedContainer(
                      duration: _duracion,
                      curve: Curves.easeOut,
                      height: 3,
                      width: activa ? 44 : 22,
                      decoration: BoxDecoration(
                        color: activa
                            ? AppColors.primary
                            : Colors.black.withValues(alpha: 0.12),
                        borderRadius: BorderRadius.circular(2),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildIcono(bool activa) {
    return AnimatedContainer(
      duration: _duracion,
      curve: Curves.easeOut,
      width: 76,
      height: 76,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        gradient: activa
            ? const LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: [AppColors.primaryLight, AppColors.primaryDark],
              )
            : LinearGradient(
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
                colors: [
                  AppColors.primary.withValues(alpha: 0.12),
                  AppColors.primary.withValues(alpha: 0.05),
                ],
              ),
        boxShadow: activa
            ? [
                BoxShadow(
                  color: AppColors.primary.withValues(alpha: 0.35),
                  blurRadius: 16,
                  offset: const Offset(0, 6),
                ),
              ]
            : null,
      ),
      child: Icon(
        widget.icon,
        size: 36,
        color: activa ? Colors.white : AppColors.primary,
      ),
    );
  }
}
