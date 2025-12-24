import api from './api';
import type { ConfiguracionCalculo } from '../types';

export const configuracionesService = {
  getAll: async (): Promise<ConfiguracionCalculo[]> => {
    const response = await api.get('/ConfiguracionCalculo');
    return response.data;
  },

  getById: async (id: number): Promise<ConfiguracionCalculo> => {
    const response = await api.get(`/ConfiguracionCalculo/${id}`);
    return response.data;
  },

  getBySector: async (sectorId: number): Promise<ConfiguracionCalculo[]> => {
    const response = await api.get(`/ConfiguracionCalculo/sector/${sectorId}`);
    return response.data;
  },

  getBySectorYTemporada: async (sectorId: number, esVerano: boolean): Promise<ConfiguracionCalculo> => {
    const response = await api.get(`/ConfiguracionCalculo/sector/${sectorId}/temporada/${esVerano}`);
    return response.data;
  },

  create: async (configuracion: Omit<ConfiguracionCalculo, 'idConfiguracion' | 'fechaCreacion' | 'fechaModificacion'>): Promise<ConfiguracionCalculo> => {
    const response = await api.post('/ConfiguracionCalculo', configuracion);
    return response.data;
  },

  update: async (id: number, configuracion: Omit<ConfiguracionCalculo, 'fechaCreacion' | 'fechaModificacion'>): Promise<void> => {
    await api.put(`/ConfiguracionCalculo/${id}`, configuracion);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/ConfiguracionCalculo/${id}`);
  },
};
