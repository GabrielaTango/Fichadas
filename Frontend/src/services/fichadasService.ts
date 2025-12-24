import api from './api';
import type { Fichada, ExportarFichadasRequest, ExportarFichadasResult } from '../types';

export const fichadasService = {
  getAll: async (): Promise<Fichada[]> => {
    const response = await api.get<Fichada[]>('/fichadas');
    return response.data;
  },

  getById: async (id: number): Promise<Fichada> => {
    const response = await api.get<Fichada>(`/fichadas/${id}`);
    return response.data;
  },

  getByEmpleado: async (empleadoId: number): Promise<Fichada[]> => {
    const response = await api.get<Fichada[]>(`/fichadas/empleado/${empleadoId}`);
    return response.data;
  },

  getByRangoFechas: async (fechaDesde: string, fechaHasta: string): Promise<Fichada[]> => {
    const response = await api.get<Fichada[]>('/fichadas/rango', {
      params: { fechaDesde, fechaHasta }
    });
    return response.data;
  },

  getByFiltros: async (
    fechaDesde?: string,
    fechaHasta?: string,
    busquedaEmpleado?: string,
    exportada?: boolean
  ): Promise<Fichada[]> => {
    const response = await api.get<Fichada[]>('/fichadas/filtros', {
      params: {
        fechaDesde: fechaDesde || undefined,
        fechaHasta: fechaHasta || undefined,
        busquedaEmpleado: busquedaEmpleado || undefined,
        exportada: exportada !== undefined ? exportada : undefined,
      }
    });
    return response.data;
  },

  create: async (fichada: Omit<Fichada, 'idFichadas'>): Promise<{ id: number }> => {
    const response = await api.post<{ id: number }>('/fichadas', fichada);
    return response.data;
  },

  importar: async (archivo: File): Promise<{
    message: string;
    fichadasImportadas: number;
    fichadasIgnoradas: number;
    errores?: string[]
  }> => {
    const formData = new FormData();
    formData.append('archivo', archivo);

    const response = await api.post('/fichadas/importar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  update: async (id: number, fichada: Omit<Fichada, 'idFichadas'>): Promise<void> => {
    await api.put(`/fichadas/${id}`, fichada);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/fichadas/${id}`);
  },

  recalcular: async (id: number): Promise<{
    message: string;
    horasTotales: number;
    trabajadas: number;
    extras: number;
    adicionales: number;
    advertencias: string[];
  }> => {
    const response = await api.post(`/fichadas/${id}/recalcular`);
    return response.data;
  },

  exportar: async (idsFichadas: number[]): Promise<ExportarFichadasResult> => {
    const request: ExportarFichadasRequest = { idsFichadas };
    const response = await api.post<ExportarFichadasResult>('/fichadas/exportar', request);
    return response.data;
  },

  descargarExcel: async (
    fechaDesde?: string,
    fechaHasta?: string,
    busquedaEmpleado?: string,
    exportada?: boolean
  ): Promise<Blob> => {
    const response = await api.get('/fichadas/descargar-excel', {
      params: {
        fechaDesde: fechaDesde || undefined,
        fechaHasta: fechaHasta || undefined,
        busquedaEmpleado: busquedaEmpleado || undefined,
        exportada: exportada !== undefined ? exportada : undefined,
      },
      responseType: 'blob',
    });
    return response.data;
  },
};
