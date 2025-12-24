import api from './api';
import type { Sector } from '../types';

export const sectoresService = {
  getAll: async (): Promise<Sector[]> => {
    const response = await api.get<Sector[]>('/sectores');
    return response.data;
  },

  getById: async (id: number): Promise<Sector> => {
    const response = await api.get<Sector>(`/sectores/${id}`);
    return response.data;
  },

  create: async (sector: Omit<Sector, 'idSector'>): Promise<{ id: number }> => {
    const response = await api.post<{ id: number }>('/sectores', sector);
    return response.data;
  },

  update: async (id: number, sector: Omit<Sector, 'idSector'>): Promise<void> => {
    await api.put(`/sectores/${id}`, sector);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/sectores/${id}`);
  },
};
