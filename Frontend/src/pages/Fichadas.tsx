import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fichadasService } from '../services/fichadasService';
import { empleadosService } from '../services/empleadosService';
import { novedadesService } from '../services/novedadesService';
import { Link } from 'react-router-dom';
import Swal from 'sweetalert2';
import Select from 'react-select';
import type { Fichada, FichadaFormData } from '../types';
import { useAuthStore } from '../stores/authStore';

export const Fichadas = () => {
  // Estados para los inputs del filtro
  const [fechaDesdeInput, setFechaDesdeInput] = useState('');
  const [fechaHastaInput, setFechaHastaInput] = useState('');
  const [busquedaEmpleadoInput, setBusquedaEmpleadoInput] = useState('');
  const [exportadaInput, setExportadaInput] = useState<boolean | undefined>(undefined);

  // Estados para los filtros aplicados
  const [fechaDesdeAplicada, setFechaDesdeAplicada] = useState('');
  const [fechaHastaAplicada, setFechaHastaAplicada] = useState('');
  const [busquedaEmpleadoAplicada, setBusquedaEmpleadoAplicada] = useState('');
  const [exportadaAplicada, setExportadaAplicada] = useState<boolean | undefined>(undefined);

  const [showModal, setShowModal] = useState(false);
  const [editingFichada, setEditingFichada] = useState<Fichada | null>(null);

  // Estados para selección y exportación
  const [selectedFichadas, setSelectedFichadas] = useState<number[]>([]);
  const [formData, setFormData] = useState<FichadaFormData>({
    empleadoId: 0,
    fecha: '',
    horaEntrada: '',
    horaSalida: '',
    codigoNovedad: '',
    novedadId: undefined,
  });

  const queryClient = useQueryClient();
  const user = useAuthStore((state) => state.user);
  const isAdmin = user?.esAdmin || false;

  // Detectar si es turno nocturno (salida menor que entrada)
  const esTurnoNocturno = (): boolean => {
    if (!formData.horaEntrada || !formData.horaSalida) return false;
    return formData.horaSalida < formData.horaEntrada;
  };

  // Query para obtener fichadas (reactivo a los filtros aplicados)
  const { data: fichadas, isLoading } = useQuery({
    queryKey: ['fichadas', fechaDesdeAplicada, fechaHastaAplicada, busquedaEmpleadoAplicada, exportadaAplicada],
    queryFn: () => {
      // Si hay algún filtro aplicado, usar el endpoint de filtros
      if (fechaDesdeAplicada || fechaHastaAplicada || busquedaEmpleadoAplicada || exportadaAplicada !== undefined) {
        return fichadasService.getByFiltros(
          fechaDesdeAplicada || undefined,
          fechaHastaAplicada || undefined,
          busquedaEmpleadoAplicada || undefined,
          exportadaAplicada
        );
      }
      return fichadasService.getAll();
    },
  });

  // Query para obtener empleados
  const { data: empleados } = useQuery({
    queryKey: ['empleados'],
    queryFn: empleadosService.getAll,
  });

  // Query para obtener novedades
  const { data: novedades } = useQuery({
    queryKey: ['novedades'],
    queryFn: novedadesService.getAll,
  });

  // Preparar opciones para el Select de empleados (memoizado para performance)
  const empleadoOptions = useMemo(() => {
    return empleados?.map((empleado) => ({
      value: empleado.idEmpleado,
      label: `${empleado.nombre} - Legajo: ${empleado.legajo} (${empleado.sectorNombre || 'Sin sector'})`,
      empleado: empleado,
    })) || [];
  }, [empleados]);

  // Valor seleccionado del empleado
  const empleadoSeleccionadoOption = useMemo(() => {
    if (!formData.empleadoId) return null;
    return empleadoOptions.find(opt => opt.value === formData.empleadoId) || null;
  }, [formData.empleadoId, empleadoOptions]);

  // Preparar opciones para el Select de novedades (memoizado para performance)
  const novedadOptions = useMemo(() => {
    return novedades?.map((novedad) => ({
      value: novedad.idNovedad,
      label: `${novedad.codNovedad} - ${novedad.descNovedad}`,
      novedad: novedad,
    })) || [];
  }, [novedades]);

  // Valor seleccionado de la novedad
  const novedadSeleccionadaOption = useMemo(() => {
    if (!formData.novedadId) return null;
    return novedadOptions.find(opt => opt.value === formData.novedadId) || null;
  }, [formData.novedadId, novedadOptions]);

  // Calcular totales de las fichadas filtradas
  const totales = useMemo(() => {
    if (!fichadas || fichadas.length === 0) {
      return {
        totalMinutos: 0,
        trabajadasMinutos: 0,
        extrasMinutos: 0,
        adicionalesMinutos: 0,
        totalHoras: 0,
        trabajadasHoras: 0,
        extrasHoras: 0,
        adicionalesHoras: 0,
      };
    }

    const totalMinutos = fichadas.reduce((sum, f) => sum + (f.horasTotales || 0), 0);
    const trabajadasMinutos = fichadas.reduce((sum, f) => sum + (f.trabajadas || 0), 0);
    const extrasMinutos = fichadas.reduce((sum, f) => sum + (f.extras || 0), 0);
    const adicionalesMinutos = fichadas.reduce((sum, f) => sum + (f.adicionales || 0), 0);

    return {
      totalMinutos,
      trabajadasMinutos,
      extrasMinutos,
      adicionalesMinutos,
      totalHoras: Math.floor(totalMinutos / 60),
      trabajadasHoras: Math.floor(trabajadasMinutos / 60),
      extrasHoras: Math.floor(extrasMinutos / 60),
      adicionalesHoras: Math.floor(adicionalesMinutos / 60),
    };
  }, [fichadas]);

  // Mutation para crear fichada
  const createMutation = useMutation({
    mutationFn: (data: Omit<Fichada, 'idFichadas'>) => fichadasService.create(data),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['fichadas'] });
      setShowModal(false);
      resetForm();
      Swal.fire('¡Éxito!', 'Fichada creada correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al crear fichada', 'error');
    },
  });

  // Mutation para actualizar fichada
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Omit<Fichada, 'idFichadas'> }) =>
      fichadasService.update(id, data),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['fichadas'] });
      setShowModal(false);
      resetForm();
      Swal.fire('¡Éxito!', 'Fichada actualizada correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al actualizar fichada', 'error');
    },
  });

  // Mutation para eliminar fichada
  const deleteMutation = useMutation({
    mutationFn: (id: number) => fichadasService.delete(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['fichadas'] });
      Swal.fire('¡Éxito!', 'Fichada eliminada correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al eliminar fichada', 'error');
    },
  });

  // Mutation para recalcular fichada
  const recalcularMutation = useMutation({
    mutationFn: (id: number) => fichadasService.recalcular(id),
    onSuccess: async (data) => {
      await queryClient.invalidateQueries({ queryKey: ['fichadas'] });

      let message = data.message;
      if (data.advertencias && data.advertencias.length > 0) {
        message += '\n\nAdvertencias:\n' + data.advertencias.join('\n');
      }

      Swal.fire('¡Éxito!', message, 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al recalcular fichada', 'error');
    },
  });

  // Mutation para exportar fichadas
  const exportarMutation = useMutation({
    mutationFn: (ids: number[]) => fichadasService.exportar(ids),
    onSuccess: async (data) => {
      await queryClient.invalidateQueries({ queryKey: ['fichadas'] });
      setSelectedFichadas([]);

      let message = data.message;
      if (data.advertencias && data.advertencias.length > 0) {
        message += '\n\nAdvertencias:\n' + data.advertencias.join('\n');
      }
      if (data.errores && data.errores.length > 0) {
        message += '\n\nErrores:\n' + data.errores.join('\n');
      }

      const icon = data.fichadasExportadas > 0 ? 'success' : 'error';
      Swal.fire('Exportación Completada', message, icon);
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al exportar fichadas', 'error');
    },
  });

  const resetForm = () => {
    setFormData({
      empleadoId: 0,
      fecha: '',
      horaEntrada: '',
      horaSalida: '',
      codigoNovedad: '',
      novedadId: undefined,
    });
    setEditingFichada(null);
  };

  const handleOpenModal = (fichada?: Fichada) => {
    if (fichada) {
      // Editar
      setEditingFichada(fichada);
      const entrada = new Date(fichada.horaEntrada);
      const salida = fichada.horaSalida ? new Date(fichada.horaSalida) : null;

      setFormData({
        empleadoId: fichada.empleadoId,
        fecha: entrada.toISOString().split('T')[0],
        horaEntrada: entrada.toTimeString().substring(0, 5),
        horaSalida: salida ? salida.toTimeString().substring(0, 5) : '',
        codigoNovedad: fichada.codigoNovedad || '',
        novedadId: fichada.novedadId || undefined,
      });
    } else {
      // Crear nueva
      resetForm();
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    resetForm();
  };

  // Función para formatear fecha a ISO string sin conversión a UTC
  const toLocalISOString = (date: Date): string => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.empleadoId || !formData.fecha || !formData.horaEntrada) {
      Swal.fire('Error', 'Complete los campos obligatorios', 'error');
      return;
    }

    // Combinar fecha y hora
    const horaEntrada = new Date(`${formData.fecha}T${formData.horaEntrada}:00`);
    let horaSalida: Date | undefined = undefined;

    if (formData.horaSalida) {
      horaSalida = new Date(`${formData.fecha}T${formData.horaSalida}:00`);

      // Si la hora de salida es menor que la entrada, significa que salió al día siguiente
      if (horaSalida < horaEntrada) {
        horaSalida.setDate(horaSalida.getDate() + 1);
      }
    }

    const fichadaData: Omit<Fichada, 'idFichadas'> = {
      empleadoId: formData.empleadoId,
      horaEntrada: toLocalISOString(horaEntrada),
      horaSalida: horaSalida ? toLocalISOString(horaSalida) : undefined,
      codigoNovedad: formData.codigoNovedad || undefined,
      novedadId: formData.novedadId || undefined,
      exportada: false,
    };

    if (editingFichada) {
      updateMutation.mutate({ id: editingFichada.idFichadas, data: fichadaData });
    } else {
      createMutation.mutate(fichadaData);
    }
  };

  const handleDelete = (id: number) => {
    Swal.fire({
      title: '¿Está seguro?',
      text: 'Esta acción no se puede deshacer',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        deleteMutation.mutate(id);
      }
    });
  };

  const handleRecalcular = (id: number) => {
    Swal.fire({
      title: '¿Recalcular horas?',
      text: 'Esto recalculará las horas normales, extras y adicionales basándose en la configuración actual',
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, recalcular',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        recalcularMutation.mutate(id);
      }
    });
  };

  const handleToggleSelect = (id: number) => {
    setSelectedFichadas((prev) =>
      prev.includes(id) ? prev.filter((fichadaId) => fichadaId !== id) : [...prev, id]
    );
  };

  const handleToggleSelectAll = () => {
    // Solo seleccionar fichadas no exportadas
    const fichadasNoExportadas = fichadas?.filter((f) => !f.exportada) || [];

    if (selectedFichadas.length === fichadasNoExportadas.length && fichadasNoExportadas.length > 0) {
      setSelectedFichadas([]);
    } else {
      setSelectedFichadas(fichadasNoExportadas.map((f) => f.idFichadas));
    }
  };

  const handleExportar = () => {
    if (selectedFichadas.length === 0) {
      Swal.fire('Error', 'Debe seleccionar al menos una fichada para exportar', 'error');
      return;
    }

    Swal.fire({
      title: '¿Exportar fichadas a Tango?',
      text: `Se exportarán ${selectedFichadas.length} fichadas seleccionadas`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#28a745',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, exportar',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        exportarMutation.mutate(selectedFichadas);
      }
    });
  };

  const handleFiltrar = () => {
    // Si hay rango de fechas, validar que ambas estén completas
    if ((fechaDesdeInput && !fechaHastaInput) || (!fechaDesdeInput && fechaHastaInput)) {
      Swal.fire('Error', 'Debe ingresar ambas fechas (Desde y Hasta) o ninguna', 'error');
      return;
    }

    // Validar que fechaDesde <= fechaHasta si ambas están presentes
    if (fechaDesdeInput && fechaHastaInput) {
      const desde = new Date(fechaDesdeInput);
      const hasta = new Date(fechaHastaInput);

      if (desde > hasta) {
        Swal.fire('Error', 'La fecha "Desde" no puede ser mayor que la fecha "Hasta"', 'error');
        return;
      }
    }

    // Aplicar filtros (la query se ejecutará automáticamente al cambiar estas dependencias)
    setFechaDesdeAplicada(fechaDesdeInput);
    setFechaHastaAplicada(fechaHastaInput);
    setBusquedaEmpleadoAplicada(busquedaEmpleadoInput);
    setExportadaAplicada(exportadaInput);
  };

  const handleLimpiarFiltro = () => {
    setFechaDesdeInput('');
    setFechaHastaInput('');
    setBusquedaEmpleadoInput('');
    setExportadaInput(undefined);
    setFechaDesdeAplicada('');
    setFechaHastaAplicada('');
    setBusquedaEmpleadoAplicada('');
    setExportadaAplicada(undefined);
  };

  const handleDescargarExcel = async () => {
    try {
      Swal.fire({
        title: 'Generando Excel...',
        text: 'Por favor espere',
        allowOutsideClick: false,
        didOpen: () => {
          Swal.showLoading();
        },
      });

      const blob = await fichadasService.descargarExcel(
        fechaDesdeAplicada || undefined,
        fechaHastaAplicada || undefined,
        busquedaEmpleadoAplicada || undefined,
        exportadaAplicada
      );

      // Crear un enlace temporal para descargar el archivo
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `Fichadas_${new Date().toISOString().split('T')[0]}.xlsx`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      Swal.fire('¡Éxito!', 'Archivo Excel descargado correctamente', 'success');
    } catch (error: any) {
      console.error('Error al descargar Excel:', error);
      Swal.fire('Error', error.response?.data?.message || 'Error al generar el archivo Excel', 'error');
    }
  };

  const getEmpleadoNombre = (fichada: Fichada): string => {
    if (fichada.empleadoNombre && fichada.empleadoLegajo) {
      return `${fichada.empleadoNombre} (Legajo: ${fichada.empleadoLegajo})`;
    }
    const empleado = empleados?.find((e) => e.idEmpleado === fichada.empleadoId);
    return empleado ? `${empleado.nombre} (Legajo: ${empleado.legajo})` : `ID: ${fichada.empleadoId}`;
  };

  /*
    const formatDateTime = (dateString: string | undefined): string => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }; 
  */

    const formatTime = (dateString: string | undefined): string => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('es-AR', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

      const formatDate = (dateString: string | undefined): string => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

  const convertMinutesToHours = (minutes: number | undefined): string => {
    if (minutes === undefined || minutes === null) return '-';
    const hours = Math.floor(minutes / 60);
    //const mins = minutes % 60;
    return `${hours}h`; //${mins}m
  };

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Gestión de Fichadas</h1>
        <div>
          <button
            className="btn btn-info me-2"
            onClick={handleDescargarExcel}
            disabled={!fichadas || fichadas.length === 0}
            title="Descargar fichadas filtradas a Excel"
          >
            <i className="bi bi-file-earmark-excel me-2"></i>
            Descargar Excel
          </button>
          {selectedFichadas.length > 0 && (
            <button
              className="btn btn-success me-2"
              onClick={handleExportar}
              disabled={exportarMutation.isPending}
            >
              {exportarMutation.isPending ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                  Exportando...
                </>
              ) : (
                <>
                  <i className="bi bi-upload me-2"></i>
                  Exportar ({selectedFichadas.length})
                </>
              )}
            </button>
          )}
          <button className="btn btn-primary me-2" onClick={() => handleOpenModal()}>
            + Nueva Fichada
          </button>
          <Link to="/importar-fichadas" className="btn btn-success">
            Importar desde Excel
          </Link>
        </div>
      </div>

      {/* Filtros */}
      <div className="card mb-3">
        <div className="card-body">
          <div className="row g-3 align-items-end">
            <div className="col-md-2">
              <label htmlFor="fecha-desde" className="form-label">
                Desde:
              </label>
              <input
                id="fecha-desde"
                type="date"
                className="form-control"
                value={fechaDesdeInput}
                onChange={(e) => setFechaDesdeInput(e.target.value)}
              />
            </div>
            <div className="col-md-2">
              <label htmlFor="fecha-hasta" className="form-label">
                Hasta:
              </label>
              <input
                id="fecha-hasta"
                type="date"
                className="form-control"
                value={fechaHastaInput}
                onChange={(e) => setFechaHastaInput(e.target.value)}
              />
            </div>
            <div className="col-md-3">
              <label htmlFor="busqueda-empleado" className="form-label">
                Empleado (Nombre/Legajo):
              </label>
              <input
                id="busqueda-empleado"
                type="text"
                className="form-control"
                placeholder="Buscar por nombre o legajo"
                value={busquedaEmpleadoInput}
                onChange={(e) => setBusquedaEmpleadoInput(e.target.value)}
              />
            </div>
            <div className="col-md-2">
              <label htmlFor="filtro-exportada" className="form-label">
                Estado Exportación:
              </label>
              <select
                id="filtro-exportada"
              className="form-select"
                value={exportadaInput === undefined ? '' : exportadaInput ? 'true' : 'false'}
                onChange={(e) => {
                  if (e.target.value === '') {
                    setExportadaInput(undefined);
                  } else {
                    setExportadaInput(e.target.value === 'true');
                  }
                }}
              >
                <option value="">Todas</option>
                <option value="true">Exportadas</option>
                <option value="false">No Exportadas</option>
              </select>
            </div>
            <div className="col-md-1">
              <button className="btn btn-primary w-100" onClick={handleFiltrar}>
                <i className="bi bi-funnel"></i>
              </button>
            </div>
            <div className="col-md-2">
              <button className="btn btn-secondary w-100" onClick={handleLimpiarFiltro}>
                <i className="bi bi-x-circle me-2"></i>
                Limpiar
              </button>
            </div>
          </div>

          {(fechaDesdeAplicada || fechaHastaAplicada || busquedaEmpleadoAplicada || exportadaAplicada !== undefined) && (
            <div className="alert alert-info mt-3 mb-0">
              <i className="bi bi-info-circle me-2"></i>
              <strong>Filtros activos:</strong>{' '}
              {fechaDesdeAplicada && fechaHastaAplicada && (
                <span>
                  Fechas: {new Date(fechaDesdeAplicada).toLocaleDateString('es-AR')} - {new Date(fechaHastaAplicada).toLocaleDateString('es-AR')}
                  {' | '}
                </span>
              )}
              {busquedaEmpleadoAplicada && (
                <span>
                  Empleado: {busquedaEmpleadoAplicada}
                  {' | '}
                </span>
              )}
              {exportadaAplicada !== undefined && (
                <span>
                  Estado: {exportadaAplicada ? 'Exportadas' : 'No Exportadas'}
                </span>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Totalizador de horas */}
      {fichadas && fichadas.length > 0 && (
        <div className="card mb-3">
          <div className="card-header bg-primary text-white">
            <h5 className="mb-0">
              <i className="bi bi-calculator me-2"></i>
              Totales de Fichadas{' '}
              {(fechaDesdeAplicada || fechaHastaAplicada || busquedaEmpleadoAplicada || exportadaAplicada !== undefined) && '(Filtradas)'}
            </h5>
          </div>
          <div className="card-body">
            <div className="row text-center">
              <div className="col-md-3">
                <div className="p-3 border rounded">
                  <h6 className="text-muted">Total de Fichadas</h6>
                  <h3 className="mb-0">{fichadas.length}</h3>
                </div>
              </div>
              <div className="col-md-2">
                <div className="p-3 border rounded bg-light">
                  <h6 className="text-muted">Horas Totales</h6>
                  <h3 className="mb-0 text-primary">{totales.totalHoras}h</h3>
                </div>
              </div>
              <div className="col-md-2">
                <div className="p-3 border rounded bg-light">
                  <h6 className="text-muted">Horas Trabajadas</h6>
                  <h3 className="mb-0 text-success">{totales.trabajadasHoras}h</h3>
                </div>
              </div>
              <div className="col-md-2">
                <div className="p-3 border rounded bg-light">
                  <h6 className="text-muted">Horas Extras</h6>
                  <h3 className="mb-0 text-warning">{totales.extrasHoras}h</h3>
                </div>
              </div>
              <div className="col-md-3">
                <div className="p-3 border rounded bg-light">
                  <h6 className="text-muted">Horas Adicionales</h6>
                  <h3 className="mb-0 text-info">{totales.adicionalesHoras}h</h3>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Tabla de fichadas */}
      {isLoading ? (
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
                    <th>
                      <input
                        type="checkbox"
                        checked={
                          fichadas &&
                          fichadas.filter((f) => !f.exportada).length > 0 &&
                          selectedFichadas.length === fichadas.filter((f) => !f.exportada).length
                        }
                        onChange={handleToggleSelectAll}
                        title="Seleccionar todas (no exportadas)"
                      />
                    </th>
                    <th>Empleado</th>
                    <th>Fecha</th>
                    <th>Entrada</th>
                    <th>Salida</th>
                    <th>Total</th>
                    <th>Hs</th>
                    <th>Ex</th>
                    <th>Ad</th>
                    {
                      //<th>Novedad</th>
                    }
                    <th>Novedad</th>
                    <th>Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {fichadas?.map((fichada) => (
                    <tr key={fichada.idFichadas}>
                      <td>
                        <input
                          type="checkbox"
                          checked={selectedFichadas.includes(fichada.idFichadas)}
                          onChange={() => handleToggleSelect(fichada.idFichadas)}
                          disabled={fichada.exportada}
                          title={fichada.exportada ? 'Fichada ya exportada' : 'Seleccionar para exportar'}
                        />
                      </td>
                      <td>{getEmpleadoNombre(fichada)}</td>
                      <td>{formatDate(fichada.horaEntrada)}</td>
                      <td>{formatTime(fichada.horaEntrada)}</td>
                      <td>{formatTime(fichada.horaSalida)}</td>
                      <td>{convertMinutesToHours(fichada.horasTotales)}</td>
                      <td>{convertMinutesToHours(fichada.trabajadas)}</td>
                      <td>{convertMinutesToHours(fichada.extras)}</td>
                      <td>{convertMinutesToHours(fichada.adicionales)}</td>
                      { // <td>{fichada.codigoNovedad || '-'}</td>
                }
                      <td>
                        {fichada.novedadDescripcion ? (
                          <span
                            className="badge bg-info text-dark"
                            title={`Código ${fichada.novedadCodigo}`}
                          >
                            {fichada.novedadDescripcion}
                          </span>
                        ) : (
                          '-'
                        )}
                      </td>
                      <td>
                        {fichada.exportada ? (
                          <span className="badge bg-success" title={`Exportada: ${formatDate(fichada.fechaExportacion)}`}>
                            Exportada
                          </span>
                        ) : (
                          <span className="badge bg-secondary">Pendiente</span>
                        )}
                      </td>
                      <td>
                        <button
                          className="btn btn-sm btn-info me-1"
                          title={fichada.exportada ? "No se puede recalcular (exportada)" : "Recalcular horas"}
                          onClick={() => handleRecalcular(fichada.idFichadas)}
                          disabled={fichada.exportada}
                        >
                          <i className="bi bi-calculator"></i>
                        </button>
                        <button
                          className="btn btn-sm btn-warning me-1"
                          onClick={() => handleOpenModal(fichada)}
                          title={fichada.exportada ? "No se puede editar (exportada)" : "Editar"}
                          disabled={fichada.exportada}
                        >
                          <i className="bi bi-pencil"></i>
                        </button>
                        {isAdmin && (
                          <button
                            className="btn btn-sm btn-danger"
                            onClick={() => handleDelete(fichada.idFichadas)}
                            title={fichada.exportada ? "No se puede eliminar (exportada)" : "Eliminar"}
                            disabled={fichada.exportada}
                          >
                            <i className="bi bi-trash"></i>
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {fichadas?.length === 0 && (
                <div className="alert alert-info">No se encontraron fichadas</div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Modal para crear/editar fichada */}
      {showModal && (
        <div
          className="modal show d-block"
          tabIndex={-1}
          style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
        >
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editingFichada ? 'Editar Fichada' : 'Nueva Fichada'}
                </h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={handleCloseModal}
                ></button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label htmlFor="empleado" className="form-label">
                      Empleado *
                    </label>
                    <Select
                      id="empleado"
                      options={empleadoOptions}
                      value={empleadoSeleccionadoOption}
                      onChange={(option) => {
                        const empleadoId = option?.value || 0;
                        const empleadoSeleccionado = option?.empleado;

                        // Autocompletar horarios si el empleado los tiene definidos
                        const horaEntrada = empleadoSeleccionado?.horarioEntrada
                          ? empleadoSeleccionado.horarioEntrada.substring(0, 5)
                          : formData.horaEntrada;
                        const horaSalida = empleadoSeleccionado?.horarioSalida
                          ? empleadoSeleccionado.horarioSalida.substring(0, 5)
                          : formData.horaSalida;

                        setFormData({
                          ...formData,
                          empleadoId,
                          horaEntrada,
                          horaSalida,
                        });
                      }}
                      placeholder="Buscar empleado..."
                      isClearable
                      noOptionsMessage={() => 'No se encontraron empleados'}
                      styles={{
                        control: (base) => ({
                          ...base,
                          minHeight: '38px',
                          borderColor: '#dee2e6',
                          '&:hover': {
                            borderColor: '#86b7fe',
                          },
                        }),
                        menu: (base) => ({
                          ...base,
                          zIndex: 9999,
                        }),
                      }}
                      theme={(theme) => ({
                        ...theme,
                        borderRadius: 6,
                        colors: {
                          ...theme.colors,
                          primary: '#0d6efd',
                          primary25: '#e7f1ff',
                          primary50: '#cfe2ff',
                        },
                      })}
                    />
                  </div>

                  <div className="mb-3">
                    <label htmlFor="fecha" className="form-label">
                      Fecha *
                    </label>
                    <input
                      id="fecha"
                      type="date"
                      className="form-control"
                      value={formData.fecha}
                      onChange={(e) => setFormData({ ...formData, fecha: e.target.value })}
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <label htmlFor="horaEntrada" className="form-label">
                      Hora de Entrada *
                    </label>
                    <input
                      id="horaEntrada"
                      type="time"
                      className="form-control"
                      value={formData.horaEntrada}
                      onChange={(e) =>
                        setFormData({ ...formData, horaEntrada: e.target.value })
                      }
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <label htmlFor="horaSalida" className="form-label">
                      Hora de Salida
                    </label>
                    <input
                      id="horaSalida"
                      type="time"
                      className="form-control"
                      value={formData.horaSalida}
                      onChange={(e) =>
                        setFormData({ ...formData, horaSalida: e.target.value })
                      }
                    />
                  </div>

                  <div className="mb-3">
                    <label htmlFor="novedad" className="form-label">
                      Novedad
                    </label>
                    <Select
                      id="novedad"
                      options={novedadOptions}
                      value={novedadSeleccionadaOption}
                      onChange={(option) => {
                        setFormData({
                          ...formData,
                          novedadId: option?.value || undefined,
                          codigoNovedad: option?.novedad.codNovedad || '',
                        });
                      }}
                      placeholder="Buscar novedad..."
                      isClearable
                      noOptionsMessage={() => 'No se encontraron novedades'}
                      styles={{
                        control: (base) => ({
                          ...base,
                          minHeight: '38px',
                          borderColor: '#dee2e6',
                          '&:hover': {
                            borderColor: '#86b7fe',
                          },
                        }),
                        menu: (base) => ({
                          ...base,
                          zIndex: 9999,
                        }),
                      }}
                      theme={(theme) => ({
                        ...theme,
                        borderRadius: 6,
                        colors: {
                          ...theme.colors,
                          primary: '#0d6efd',
                          primary25: '#e7f1ff',
                          primary50: '#cfe2ff',
                        },
                      })}
                    />
                    <small className="form-text text-muted">
                      Opcional - Asignar una novedad (ej: licencia, falta, etc.)
                    </small>
                  </div>

                  {esTurnoNocturno() && (
                    <div className="alert alert-warning">
                      <i className="bi bi-moon-stars me-2"></i>
                      <strong>Turno Nocturno Detectado:</strong> La hora de salida ({formData.horaSalida}) es
                      menor que la entrada ({formData.horaEntrada}). Se registrará la salida para el día
                      siguiente automáticamente.
                    </div>
                  )}
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={handleCloseModal}>
                    Cancelar
                  </button>
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={createMutation.isPending || updateMutation.isPending}
                  >
                    {createMutation.isPending || updateMutation.isPending ? (
                      <>
                        <span
                          className="spinner-border spinner-border-sm me-2"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        Guardando...
                      </>
                    ) : (
                      'Guardar'
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
