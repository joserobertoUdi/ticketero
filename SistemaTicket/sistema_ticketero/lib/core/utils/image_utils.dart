import 'dart:io';
import 'package:flutter/material.dart';

import '../constants/sharepoint_constants.dart';

/// Devuelve un [ImageProvider] para las tres formas en que se guarda un logo:
///
/// * `sharepoint:{uid}` — archivo en el servicio central de gestión documental.
///   Requiere la cabecera ProviderKey.
/// * `http(s)://...` — URL directa.
/// * ruta local — solo válida en la máquina donde se configuró.
ImageProvider? safeImageProvider(String path) {
  if (path.isEmpty) return null;

  if (SharepointConstants.isRef(path)) {
    return NetworkImage(
      SharepointConstants.resolveUrl(path),
      headers: SharepointConstants.downloadHeaders,
    );
  }

  if (path.startsWith('http')) return NetworkImage(path);

  final file = File(path);
  if (file.existsSync()) return FileImage(file);
  return null;
}
