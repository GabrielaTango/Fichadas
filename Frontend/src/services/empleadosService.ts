import api from './api';
import type { Empleado } from '../types';

export const empleadosService = {
  getAll: async (): Promise<Empleado[]> => {
    const response = await api.get<Empleado[]>('/empleados');
    return response.data;
  },

  getById: async (id: number): Promise<Empleado> => {
    const response = await api.get<Empleado>(`/empleados/${id}`);
    return response.data;
  },

  getByLegajo: async (legajo: number): Promise<Empleado> => {
    const response = await api.get<Empleado>(`/empleados/legajo/${legajo}`);
    return response.data;
  },

  getBySector: async (sectorId: number): Promise<Empleado[]> => {
    const response = await api.get<Empleado[]>(`/empleados/sector/${sectorId}`);
    return response.data;
  },

  create: async (empleado: Omit<Empleado, 'idEmpleado' | 'sectorNombre'>): Promise<{ id: number }> => {
    const response = await api.post<{ id: number }>('/empleados', empleado);
    return response.data;
  },

  update: async (id: number, empleado: Omit<Empleado, 'idEmpleado' | 'sectorNombre'>): Promise<void> => {
    await api.put(`/empleados/${id}`, empleado);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/empleados/${id}`);
  },

  importarDesdeTango: async (): Promise<{
    message: string;
    empleadosImportados: number;
    empleadosActualizados: number;
    empleadosExistentes: number;
    errores?: string[];
  }> => {
    const response = await api.post('/empleados/importar-desde-tango');
    return response.data;
  },
};
