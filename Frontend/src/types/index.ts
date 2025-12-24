// Tipos de la aplicación

export interface Usuario {
  idUsuario: number;
  usuario: string;
  mail: string;
  esAdmin: boolean;
}

export interface UsuarioFormData {
  usuario: string;
  password?: string;
  mail: string;
  esAdmin: boolean;
}

export interface LoginRequest {
  usuario: string;
  password: string;
}

export interface LoginResponse {
  idUsuario: number;
  usuario: string;
  mail: string;
  esAdmin: boolean;
  token: string;
}

export interface Sector {
  idSector: number;
  nombre: string;
  esRotativo: boolean;
  novedadExtrasId?: number;
  novedadExtrasCodigo?: string;
  novedadExtrasDescripcion?: string;
  novedadTrabajadasId?: number;
  novedadTrabajadasCodigo?: string;
  novedadTrabajadasDescripcion?: string;
}

export interface Empleado {
  idEmpleado: number;
  nombre: string;
  legajo: number;
  sectorId: number;
  sectorNombre: string;
  horarioEntrada?: string;
  horarioSalida?: string;
  fechaInicioRotacion?: string;
}

export interface ConfiguracionCalculo {
  idConfiguracion: number;
  sectorId: number;
  esVerano: boolean;
  horasNormales: number;
  horasExtrasOficiales: number;
  horasExtrasAdicionales: number;
  toleranciaMinutos: number;
  descuentoTarde6a30Min: number;
  descuentoTarde31Mas: number;
  horaEntradaEsperada?: string;
  horaSalidaEsperada?: string;
  tipoTurno?: string; // 'diurno' | 'nocturno' | null
  activo: boolean;
  fechaCreacion?: string;
  fechaModificacion?: string;
}

export interface Fichada {
  idFichadas: number;
  empleadoId: number;
  horaEntrada: string;
  horaSalida?: string;
  horasTotales?: number;
  trabajadas?: number;
  extras?: number;
  adicionales?: number;
  codigoNovedad?: string;
  novedadId?: number;
  exportada: boolean;
  fechaExportacion?: string;
  // Campos adicionales para mostrar en la interfaz
  empleadoNombre?: string;
  empleadoLegajo?: number;
  novedadDescripcion?: string;
  novedadCodigo?: string;
  empleadoIdTango?: number;
  novedadIdTango?: number;
}

export interface FichadaFormData {
  empleadoId: number;
  fecha: string;
  horaEntrada: string;
  horaSalida?: string;
  codigoNovedad?: string;
  novedadId?: number;
}

export interface FichadaExcel {
  legajo: number;
  fechaHora: string;
  tipo: 'Entrada' | 'Salida';
}

export interface Novedad {
  idNovedad: number;
  codNovedad: string;
  descNovedad: string;
  fechaCreacion?: string;
  fechaModificacion?: string;
}

export interface Concepto {
  idConcepto: number;
  idConceptoTango: number;
  nroConcepto: number;
  descConcepto: string;
  fechaCreacion?: string;
  fechaModificacion?: string;
}

export interface ExportarFichadasRequest {
  idsFichadas: number[];
}

export interface ExportarFichadasResult {
  fichadasExportadas: number;
  fichadasConError: number;
  errores: string[];
  advertencias: string[];
  message: string;
}

export interface ApiError {
  message: string;
  status?: number;
}
