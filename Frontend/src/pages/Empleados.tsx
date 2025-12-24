import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import Swal from 'sweetalert2';
import { empleadosService } from '../services/empleadosService';
import { sectoresService } from '../services/sectoresService';
import type { Empleado } from '../types';
import { useAuthStore } from '../stores/authStore';

type EmpleadoFormData = {
  nombre: string;
  legajo: number;
  sectorId: number;
  fechaInicioRotacion?: string;
};

export const Empleados = () => {
  const queryClient = useQueryClient();
  const [selectedSector, setSelectedSector] = useState<number | null>(null);
  const [editingEmpleado, setEditingEmpleado] = useState<Empleado | null>(null);
  const [showForm, setShowForm] = useState(false);
  const user = useAuthStore((state) => state.user);
  const isAdmin = user?.esAdmin || false;

  const { data: empleados, isLoading: loadingEmpleados } = useQuery({
    queryKey: ['empleados', selectedSector],
    queryFn: async () => {
      // Si se selecciona "Sin sector" (-1), obtener todos y filtrar
      if (selectedSector === -1) {
        const allEmpleados = await empleadosService.getAll();
        return allEmpleados.filter(e => !e.sectorId || e.sectorId === null);
      }
      // Si se selecciona un sector específico
      if (selectedSector && selectedSector > 0) {
        return empleadosService.getBySector(selectedSector);
      }
      // Si no hay filtro, obtener todos
      return empleadosService.getAll();
    },
  });

  const { data: sectores } = useQuery({
    queryKey: ['sectores'],
    queryFn: () => sectoresService.getAll(),
  });

  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors },
  } = useForm<EmpleadoFormData>();

  // Observar el sector seleccionado para mostrar/ocultar campo de fecha rotación
  const sectorIdSeleccionado = watch('sectorId');
  const sectorSeleccionado = sectores?.find(s => s.idSector === sectorIdSeleccionado);

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Omit<Empleado, 'idEmpleado' | 'sectorNombre'> }) =>
      empleadosService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['empleados'] });
      reset();
      setEditingEmpleado(null);
      setShowForm(false);
      Swal.fire('¡Éxito!', 'Empleado actualizado exitosamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al actualizar el empleado', 'error');
    },
  });

  const importarMutation = useMutation({
    mutationFn: () => empleadosService.importarDesdeTango(),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['empleados'] });

      let message = data.message;
      if (data.errores && data.errores.length > 0) {
        message += '\n\nErrores:\n' + data.errores.join('\n');
        Swal.fire('Importación Completada con Advertencias', message, 'warning');
      } else {
        Swal.fire('¡Éxito!', message, 'success');
      }
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al importar empleados desde Tango', 'error');
    },
  });

  const onSubmit = (data: EmpleadoFormData) => {
    if (editingEmpleado) {
      updateMutation.mutate({
        id: editingEmpleado.idEmpleado,
        data: {
          nombre: data.nombre,
          legajo: data.legajo,
          sectorId: data.sectorId,
          fechaInicioRotacion: data.fechaInicioRotacion || undefined,
        }
      });
    }
  };

  const handleEdit = (empleado: Empleado) => {
    setEditingEmpleado(empleado);
    reset({
      nombre: empleado.nombre || '',
      legajo: empleado.legajo,
      sectorId: empleado.sectorId,
      fechaInicioRotacion: empleado.fechaInicioRotacion
        ? new Date(empleado.fechaInicioRotacion).toISOString().split('T')[0]
        : undefined,
    });
    setShowForm(true);
  };

  const handleCancelEdit = () => {
    setEditingEmpleado(null);
    reset();
    setShowForm(false);
  };

  const handleImportarDesdeTango = () => {
    Swal.fire({
      title: '¿Importar empleados desde Tango?',
      html: `
        <p>Esta acción importará todos los empleados desde la base de datos de Tango (DELTA3.DBO.LEGAJO).</p>
        <ul style="text-align: left; margin-top: 10px;">
          <li>Los empleados <strong>nuevos</strong> se crearán sin sector asignado</li>
          <li>Los empleados <strong>existentes</strong> se actualizarán si cambió su nombre</li>
          <li>No se borrarán empleados existentes</li>
        </ul>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, importar',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        importarMutation.mutate();
      }
    });
  };

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Gestión de Empleados</h1>
        {isAdmin && (
          <button
            className="btn btn-primary"
            onClick={handleImportarDesdeTango}
            disabled={importarMutation.isPending}
          >
            {importarMutation.isPending ? (
              <>
                <span
                  className="spinner-border spinner-border-sm me-2"
                  role="status"
                  aria-hidden="true"
                ></span>
                Importando...
              </>
            ) : (
              <>
                <i className="bi bi-database-fill-down me-2"></i>
                Importar desde Tango
              </>
            )}
          </button>
        )}
      </div>

      <div className="card mb-3">
        <div className="card-body">
          <div className="row">
            <div className="col-md-4">
              <label htmlFor="sector-filter" className="form-label">Filtrar por Sector:</label>
              <select
                id="sector-filter"
                className="form-select"
                value={selectedSector || ''}
                onChange={(e) => setSelectedSector(e.target.value ? Number(e.target.value) : null)}
              >
                <option value="">Todos los sectores</option>
                <option value="-1">Sin sector asignado</option>
                {sectores?.map((sector) => (
                  <option key={sector.idSector} value={sector.idSector}>
                    {sector.nombre}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>
      </div>

      {showForm && (
        <div className="card mb-4">
          <div className="card-body">
            <form onSubmit={handleSubmit(onSubmit)}>
              <h5 className="card-title mb-3">Editar Empleado</h5>

              <div className="row mb-3">
                <div className="col-md-6">
                  <label htmlFor="legajo" className="form-label">Legajo</label>
                  <input
                    id="legajo"
                    type="number"
                    className="form-control"
                    {...register('legajo')}
                    disabled
                  />
                </div>

                <div className="col-md-6">
                  <label htmlFor="nombre" className="form-label">Nombre</label>
                  <input
                    id="nombre"
                    type="text"
                    className="form-control"
                    {...register('nombre')}
                    disabled
                  />
                </div>
              </div>

              <div className="mb-3">
                <label htmlFor="sectorId" className="form-label">Sector *</label>
                <select
                  id="sectorId"
                  {...register('sectorId', { required: 'El sector es requerido', valueAsNumber: true })}
                  className={`form-select ${errors.sectorId ? 'is-invalid' : ''}`}
                >
                  <option value="">Seleccione un sector</option>
                  {sectores?.map((sector) => (
                    <option key={sector.idSector} value={sector.idSector}>
                      {sector.nombre} {sector.esRotativo && '(Rotativo)'}
                    </option>
                  ))}
                </select>
                {errors.sectorId && (
                  <div className="invalid-feedback">{errors.sectorId.message}</div>
                )}
              </div>

              {sectorSeleccionado?.esRotativo && (
                <div className="mb-3">
                  <label htmlFor="fechaInicioRotacion" className="form-label">
                    Fecha de Inicio de Rotación *
                  </label>
                  <input
                    id="fechaInicioRotacion"
                    type="date"
                    {...register('fechaInicioRotacion', {
                      required: sectorSeleccionado?.esRotativo ? 'La fecha de inicio es requerida para sectores rotativos' : false
                    })}
                    className={`form-control ${errors.fechaInicioRotacion ? 'is-invalid' : ''}`}
                  />
                  {errors.fechaInicioRotacion && (
                    <div className="invalid-feedback">{errors.fechaInicioRotacion.message}</div>
                  )}
                  <small className="form-text text-muted">
                    Esta fecha se usa para calcular qué turno (día/noche) le corresponde semanalmente
                  </small>
                </div>
              )}

              <div className="d-flex gap-2">
                <button type="submit" className="btn btn-primary">
                  Actualizar Sector
                </button>
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={handleCancelEdit}
                >
                  Cancelar
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {loadingEmpleados ? (
        <div className="text-center">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Cargando...</span>
          </div>
        </div>
      ) : (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
              <table className="table table-striped table-hover">
                <thead>
                  <tr>
                    <th>Legajo</th>
                    <th>Nombre</th>
                    <th>Sector</th>
                    <th>Fecha Inicio Rotación</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {empleados?.map((empleado) => (
                    <tr key={empleado.idEmpleado}>
                      <td>{empleado.legajo}</td>
                      <td>{empleado.nombre}</td>
                      <td>
                        {empleado.sectorNombre || (
                          <span className="badge bg-warning text-dark">Sin sector</span>
                        )}
                      </td>
                      <td>
                        {empleado.fechaInicioRotacion ? (
                          new Date(empleado.fechaInicioRotacion).toLocaleDateString('es-AR')
                        ) : (
                          <span className="text-muted">-</span>
                        )}
                      </td>
                      <td>
                        <button
                          className="btn btn-sm btn-outline-primary"
                          onClick={() => handleEdit(empleado)}
                        >
                          Editar Sector
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {empleados?.length === 0 && (
                <div className="alert alert-info">No se encontraron empleados</div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
