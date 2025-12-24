import api from './api';
import type { Novedad } from '../types';

export const novedadesService = {
  getAll: async (): Promise<Novedad[]> => {
    const response = await api.get<Novedad[]>('/novedades');
    return response.data;
  },

  getById: async (id: number): Promise<Novedad> => {
    const response = await api.get<Novedad>(`/novedades/${id}`);
    return response.data;
  },

  getByCodNovedad: async (codNovedad: string): Promise<Novedad> => {
    const response = await api.get<Novedad>(`/novedades/codigo/${codNovedad}`);
    return response.data;
  },

  create: async (novedad: Omit<Novedad, 'idNovedad' | 'fechaCreacion' | 'fechaModificacion'>): Promise<{ id: number }> => {
    const response = await api.post<{ id: number }>('/novedades', novedad);
    return response.data;
  },

  update: async (id: number, novedad: Omit<Novedad, 'idNovedad' | 'fechaCreacion' | 'fechaModificacion'>): Promise<void> => {
    await api.put(`/novedades/${id}`, novedad);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/novedades/${id}`);
  },

  importarDesdeTango: async (): Promise<{
    message: string;
    novedadesImportadas: number;
    novedadesActualizadas: number;
    novedadesExistentes: number;
    errores?: string[];
  }> => {
    const response = await api.post('/novedades/importar-desde-tango');
    return response.data;
  },

  getDisponiblesTango: async (): Promise<Array<{
    idNovedad: number;
    codNovedad: string;
    descNovedad: string;
  }>> => {
    const response = await api.get('/novedades/disponibles-tango');
    return response.data;
  },

  importarNovedadEspecifica: async (idNovedadTango: number): Promise<{ message: string }> => {
    const response = await api.post(`/novedades/importar-novedad/${idNovedadTango}`);
    return response.data;
  },
};
