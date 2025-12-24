import api from './api';
import type { Usuario, UsuarioFormData } from '../types';

export const usuariosService = {
  getAll: async (): Promise<Usuario[]> => {
    const response = await api.get<Usuario[]>('/usuarios');
    return response.data;
  },

  getById: async (id: number): Promise<Usuario> => {
    const response = await api.get<Usuario>(`/usuarios/${id}`);
    return response.data;
  },

  create: async (data: UsuarioFormData): Promise<{ id: number }> => {
    const response = await api.post<{ id: number }>('/usuarios', data);
    return response.data;
  },

  update: async (id: number, data: Omit<UsuarioFormData, 'password'>): Promise<void> => {
    await api.put(`/usuarios/${id}`, data);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/usuarios/${id}`);
  },

  changePassword: async (id: number, newPassword: string): Promise<{ message: string }> => {
    const response = await api.post<{ message: string }>(`/usuarios/${id}/change-password`, {
      newPassword,
    });
    return response.data;
  },
};
