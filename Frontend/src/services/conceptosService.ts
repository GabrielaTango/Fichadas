import api from './api';
import type { Concepto } from '../types';

export const conceptosService = {
  getAll: async (): Promise<Concepto[]> => {
    const response = await api.get<Concepto[]>('/conceptos');
    return response.data;
  },

  getById: async (id: number): Promise<Concepto> => {
    const response = await api.get<Concepto>(`/conceptos/${id}`);
    return response.data;
  },

  getByNroConcepto: async (nroConcepto: number): Promise<Concepto> => {
    const response = await api.get<Concepto>(`/conceptos/nro-concepto/${nroConcepto}`);
    return response.data;
  },

  create: async (concepto: Omit<Concepto, 'idConcepto' | 'fechaCreacion' | 'fechaModificacion'>): Promise<{ id: number }> => {
    const response = await api.post<{ id: number }>('/conceptos', concepto);
    return response.data;
  },

  update: async (id: number, concepto: Omit<Concepto, 'idConcepto' | 'fechaCreacion' | 'fechaModificacion'>): Promise<void> => {
    await api.put(`/conceptos/${id}`, concepto);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/conceptos/${id}`);
  },

  importarDesdeTango: async (): Promise<{
    message: string;
    conceptosImportados: number;
    conceptosActualizados: number;
    conceptosExistentes: number;
    errores?: string[];
  }> => {
    const response = await api.post('/conceptos/importar-desde-tango');
    return response.data;
  },

  getDisponiblesTango: async (): Promise<Array<{
    idConcepto: number;
    nroConcepto: number;
    descConcepto: string;
  }>> => {
    const response = await api.get('/conceptos/disponibles-tango');
    return response.data;
  },

  importarConceptoEspecifico: async (idConceptoTango: number): Promise<{ message: string }> => {
    const response = await api.post(`/conceptos/importar-concepto/${idConceptoTango}`);
    return response.data;
  },
};
