import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Swal from 'sweetalert2';
import { configuracionesService } from '../services/configuracionesService';
import { sectoresService } from '../services/sectoresService';
import type { ConfiguracionCalculo, Sector } from '../types';

type ConfiguracionFormData = Omit<ConfiguracionCalculo, 'idConfiguracion' | 'fechaCreacion' | 'fechaModificacion'>;

export function Configuraciones() {
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<ConfiguracionFormData>({
    sectorId: 0,
    esVerano: true,
    horasNormales: 9,
    horasExtrasOficiales: 1,
    horasExtrasAdicionales: 0,
    toleranciaMinutos: 5,
    descuentoTarde6a30Min: 30,
    descuentoTarde31Mas: 60,
    horaEntradaEsperada: '',
    horaSalidaEsperada: '',
    tipoTurno: undefined,
    activo: true,
  });

  // Queries
  const { data: configuraciones = [], isLoading } = useQuery({
    queryKey: ['configuraciones'],
    queryFn: configuracionesService.getAll,
  });

  const { data: sectores = [] } = useQuery({
    queryKey: ['sectores'],
    queryFn: sectoresService.getAll,
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: ConfiguracionFormData) => configuracionesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['configuraciones'] });
      resetForm();
      Swal.fire({
        icon: 'success',
        title: 'Éxito',
        text: 'Configuración creada exitosamente',
        timer: 2000,
        showConfirmButton: false
      });
    },
    onError: (error: any) => {
      Swal.fire({
        icon: 'error',
        title: 'Error',
        text: error.response?.data?.message || 'Error al crear configuración'
      });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Omit<ConfiguracionCalculo, 'fechaCreacion' | 'fechaModificacion'> }) =>
      configuracionesService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['configuraciones'] });
      resetForm();
      Swal.fire({
        icon: 'success',
        title: 'Éxito',
        text: 'Configuración actualizada exitosamente',
        timer: 2000,
        showConfirmButton: false
      });
    },
    onError: (error: any) => {
      Swal.fire({
        icon: 'error',
        title: 'Error',
        text: error.response?.data?.message || 'Error al actualizar configuración'
      });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: configuracionesService.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['configuraciones'] });
      Swal.fire({
        icon: 'success',
        title: 'Éxito',
        text: 'Configuración eliminada exitosamente',
        timer: 2000,
        showConfirmButton: false
      });
    },
    onError: (error: any) => {
      Swal.fire({
        icon: 'error',
        title: 'Error',
        text: error.response?.data?.message || 'Error al eliminar configuración'
      });
    },
  });

  // Handlers
  const resetForm = () => {
    setFormData({
      sectorId: 0,
      esVerano: true,
      horasNormales: 9,
      horasExtrasOficiales: 1,
      horasExtrasAdicionales: 0,
      toleranciaMinutos: 5,
      descuentoTarde6a30Min: 30,
      descuentoTarde31Mas: 60,
      horaEntradaEsperada: '',
      horaSalidaEsperada: '',
      tipoTurno: undefined,
      activo: true,
    });
    setEditingId(null);
    setShowForm(false);
  };

  const handleEdit = (configuracion: ConfiguracionCalculo) => {
    setFormData({
      sectorId: configuracion.sectorId,
      esVerano: configuracion.esVerano,
      horasNormales: configuracion.horasNormales,
      horasExtrasOficiales: configuracion.horasExtrasOficiales,
      horasExtrasAdicionales: configuracion.horasExtrasAdicionales,
      toleranciaMinutos: configuracion.toleranciaMinutos,
      descuentoTarde6a30Min: configuracion.descuentoTarde6a30Min,
      descuentoTarde31Mas: configuracion.descuentoTarde31Mas,
      horaEntradaEsperada: configuracion.horaEntradaEsperada || '',
      horaSalidaEsperada: configuracion.horaSalidaEsperada || '',
      tipoTurno: configuracion.tipoTurno,
      activo: configuracion.activo,
    });
    setEditingId(configuracion.idConfiguracion);
    setShowForm(true);
    // Scroll hacia arriba para ver el formulario
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.sectorId === 0) {
      Swal.fire({
        icon: 'warning',
        title: 'Atención',
        text: 'Debe seleccionar un sector'
      });
      return;
    }

    if (editingId) {
      updateMutation.mutate({
        id: editingId,
        data: { ...formData, idConfiguracion: editingId },
      });
    } else {
      createMutation.mutate(formData);
    }
  };

  const handleDelete = async (id: number) => {
    const result = await Swal.fire({
      icon: 'question',
      title: '¿Confirmar eliminación?',
      text: '¿Está seguro de eliminar esta configuración?',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d'
    });

    if (result.isConfirmed) {
      deleteMutation.mutate(id);
    }
  };

  const getSectorNombre = (sectorId: number): string => {
    const sector = sectores.find((s: Sector) => s.idSector === sectorId);
    return sector?.nombre || 'Desconocido';
  };

  // Obtener el sector seleccionado en el formulario
  const sectorSeleccionadoEnForm = sectores.find((s: Sector) => s.idSector === formData.sectorId);

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Cargando...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container py-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">Configuración de Cálculo de Horas</h2>
        <button
          className="btn btn-primary"
          onClick={() => {
            resetForm();
            setShowForm(true);
          }}
        >
          Nueva Configuración
        </button>
      </div>

      {showForm && (
        <div className="card mb-4">
          <div className="card-header">
            <h5 className="mb-0">{editingId ? 'Editar Configuración' : 'Nueva Configuración'}</h5>
          </div>
          <div className="card-body">
            <form onSubmit={handleSubmit}>
              <div className="row g-3">
                <div className="col-md-4">
                  <label className="form-label">Sector *</label>
                  <select
                    className="form-select"
                    value={formData.sectorId}
                    onChange={(e) => setFormData({ ...formData, sectorId: parseInt(e.target.value) })}
                    required
                  >
                    <option value={0}>Seleccionar sector...</option>
                    {sectores.map((sector: Sector) => (
                      <option key={sector.idSector} value={sector.idSector}>
                        {sector.nombre}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="col-md-4">
                  <label className="form-label">Temporada *</label>
                  <select
                    className="form-select"
                    value={formData.esVerano ? 'true' : 'false'}
                    onChange={(e) => setFormData({ ...formData, esVerano: e.target.value === 'true' })}
                  >
                    <option value="true">Verano</option>
                    <option value="false">Invierno</option>
                  </select>
                </div>

                {sectorSeleccionadoEnForm?.esRotativo && (
                  <div className="col-md-4">
                    <label className="form-label">Tipo de Turno *</label>
                    <select
                      className="form-select"
                      value={formData.tipoTurno || ''}
                      onChange={(e) => setFormData({ ...formData, tipoTurno: e.target.value || undefined })}
                      required={sectorSeleccionadoEnForm?.esRotativo}
                    >
                      <option value="">Seleccionar...</option>
                      <option value="diurno">Diurno</option>
                      <option value="nocturno">Nocturno</option>
                    </select>
                    <small className="form-text text-muted">
                      Para sectores rotativos se requiere configurar ambos turnos
                    </small>
                  </div>
                )}

                <div className={sectorSeleccionadoEnForm?.esRotativo ? 'col-md-12' : 'col-md-4'}>
                  <label className="form-label">Estado</label>
                  <div className="form-check form-switch mt-2">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      checked={formData.activo}
                      onChange={(e) => setFormData({ ...formData, activo: e.target.checked })}
                    />
                    <label className="form-check-label">
                      {formData.activo ? 'Activa' : 'Inactiva'}
                    </label>
                  </div>
                </div>

                <div className="col-md-4">
                  <label className="form-label">Horas Normales *</label>
                  <input
                    type="number"
                    className="form-control"
                    value={formData.horasNormales}
                    onChange={(e) => setFormData({ ...formData, horasNormales: parseInt(e.target.value) })                }
                    min="1"
                    max="24"
                    required
                  />
                </div>

                <div className="col-md-4">
                  <label className="form-label">Horas Extras Oficiales *</label>
                  <input
                    type="number"
                    className="form-control"
                    value={formData.horasExtrasOficiales}
                    onChange={(e) => setFormData({ ...formData, horasExtrasOficiales: parseInt(e.target.value) })}
                    min="0"
                    max="12"
                    required
                  />
                </div>

                <div className="col-md-4">
                  <label className="form-label">Horas Extras Adicionales *</label>
                  <input
                    type="number"
                    className="form-control"
                    value={formData.horasExtrasAdicionales}
                    onChange={(e) => setFormData({ ...formData, horasExtrasAdicionales: parseInt(e.target.value) })}
                    min="0"
                    max="12"
                    required
                  />
                </div>

                <div className="col-md-4">
                  <label className="form-label">Tolerancia (minutos) *</label>
                  <input
                    type="number"
                    className="form-control"
                    value={formData.toleranciaMinutos}
                    onChange={(e) => setFormData({ ...formData, toleranciaMinutos: parseInt(e.target.value) })}
                    min="0"
                    max="60"
                    required
                  />
                </div>

                <div className="col-md-4">
                  <label className="form-label">Descuento Tarde 6-30 min *</label>
                  <input
                    type="number"
                    className="form-control"
                    value={formData.descuentoTarde6a30Min}
                    onChange={(e) => setFormData({ ...formData, descuentoTarde6a30Min: parseInt(e.target.value) })}
                    min="0"
                    max="120"
                    required
                  />
                </div>

                <div className="col-md-4">
                  <label className="form-label">Descuento Tarde 31+ min *</label>
                  <input
                    type="number"
                    className="form-control"
                    value={formData.descuentoTarde31Mas}
                    onChange={(e) => setFormData({ ...formData, descuentoTarde31Mas: parseInt(e.target.value) })}
                    min="0"
                    max="120"
                    required
                  />
                </div>

                <div className="col-md-6">
                  <label className="form-label">Hora Entrada Esperada (opcional)</label>
                  <input
                    type="time"
                    className="form-control"
                    value={formData.horaEntradaEsperada}
                    onChange={(e) => setFormData({ ...formData, horaEntradaEsperada: e.target.value + ":00" })}
                  />
                </div>

                <div className="col-md-6">
                  <label className="form-label">Hora Salida Esperada (opcional)</label>
                  <input
                    type="time"
                    className="form-control"
                    value={formData.horaSalidaEsperada}
                    onChange={(e) => setFormData({ ...formData, horaSalidaEsperada: e.target.value + ":00" })}
                  />
                </div>
              </div>

              <div className="d-flex gap-2 mt-4">
                <button type="submit" className="btn btn-primary">
                  {editingId ? 'Actualizar' : 'Crear'}
                </button>
                <button type="button" className="btn btn-secondary" onClick={resetForm}>
                  Cancelar
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {!showForm && (
        <div className="card">
          <div className="card-body">
            <div className="table-responsive">
            <table className="table table-hover">
              <thead>
                <tr>
                  <th>Sector</th>
                  <th>Temporada</th>
                  <th>Turno</th>
                  <th>Normales</th>
                  <th>Extras</th>
                  <th>Adic</th>
                  <th>Tolerancia</th>
                  <th>Horario Esperado</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {configuraciones.map((config: ConfiguracionCalculo) => (
                  <tr key={config.idConfiguracion} className={config.activo ? 'table-success' : ''}>
                    <td>{getSectorNombre(config.sectorId)}</td>
                    <td>
                      <span className={`badge ${config.esVerano ? 'bg-warning' : 'bg-info'}`}>
                        {config.esVerano ? 'Verano' : 'Invierno'}
                      </span>
                    </td>
                    <td>
                      {config.tipoTurno ? (
                        <span className={`badge ${config.tipoTurno === 'diurno' ? 'bg-primary' : 'bg-dark'}`}>
                          {config.tipoTurno === 'diurno' ? 'Diurno' : 'Nocturno'}
                        </span>
                      ) : (
                        <span className="text-muted">-</span>
                      )}
                    </td>
                    <td>{config.horasNormales}h</td>
                    <td>{config.horasExtrasOficiales}h</td>
                    <td>{config.horasExtrasAdicionales}h</td>
                    <td>{config.toleranciaMinutos} min</td>
                    <td>
                      {config.horaEntradaEsperada && config.horaSalidaEsperada ? (
                        <small>
                          {config.horaEntradaEsperada} - {config.horaSalidaEsperada}
                        </small>
                      ) : (
                        <small className="text-muted">No definido</small>
                      )}
                    </td>
                    <td>
                      <span className={`badge ${config.activo ? 'bg-success' : 'bg-secondary'}`}>
                        {config.activo ? 'Activa' : 'Inactiva'}
                      </span>
                    </td>
                    <td>
                      <div className="d-flex gap-2">
                        <button
                          className="btn btn-sm btn-outline-primary"
                          onClick={() => handleEdit(config)}
                        >
                          Editar
                        </button>
                        <button
                          className="btn btn-sm btn-outline-danger"
                          onClick={() => handleDelete(config.idConfiguracion)}
                        >
                          Eliminar
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {configuraciones.length === 0 && (
            <div className="text-center py-4">
              <p className="text-muted mb-0">No hay configuraciones registradas</p>
            </div>
          )}
        </div>
          </div>
        
      )}
    </div>
  );
}
