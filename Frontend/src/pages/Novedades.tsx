import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Swal from 'sweetalert2';
import { novedadesService } from '../services/novedadesService';
import { useAuthStore } from '../stores/authStore';

type NovedadTango = {
  idNovedad: number;
  codNovedad: string;
  descNovedad: string;
};

export const Novedades = () => {
  const queryClient = useQueryClient();
  const user = useAuthStore((state) => state.user);
  const isAdmin = user?.esAdmin || false;
  const [searchTerm, setSearchTerm] = useState('');
  const [activeTab, setActiveTab] = useState<'locales' | 'tango'>('locales');

  // Query para novedades locales
  const { data: novedades, isLoading: loadingLocales } = useQuery({
    queryKey: ['novedades'],
    queryFn: novedadesService.getAll,
  });

  // Query para novedades disponibles en Tango
  const { data: novedadesTango, isLoading: loadingTango, refetch: refetchTango } = useQuery({
    queryKey: ['novedades-tango'],
    queryFn: novedadesService.getDisponiblesTango,
    enabled: activeTab === 'tango',
  });

  const importarTodosMutation = useMutation({
    mutationFn: () => novedadesService.importarDesdeTango(),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['novedades'] });
      queryClient.invalidateQueries({ queryKey: ['novedades-tango'] });

      let message = data.message;
      if (data.errores && data.errores.length > 0) {
        message += '\n\nErrores:\n' + data.errores.join('\n');
        Swal.fire('Importación Completada con Advertencias', message, 'warning');
      } else {
        Swal.fire('¡Éxito!', message, 'success');
      }
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al importar novedades desde Tango', 'error');
    },
  });

  const importarUnaMutation = useMutation({
    mutationFn: (idNovedadTango: number) => novedadesService.importarNovedadEspecifica(idNovedadTango),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['novedades'] });
      refetchTango();
      Swal.fire('¡Éxito!', 'Novedad importada correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al importar novedad', 'error');
    },
  });

  const handleImportarTodos = () => {
    Swal.fire({
      title: '¿Importar TODAS las novedades desde Tango?',
      html: `
        <p>Esta acción importará todas las novedades desde la base de datos de Tango.</p>
        <ul style="text-align: left; margin-top: 10px;">
          <li>Las novedades <strong>nuevas</strong> se crearán automáticamente</li>
          <li>Las novedades <strong>existentes</strong> se actualizarán si cambió su descripción</li>
          <li>No se borrarán novedades existentes</li>
        </ul>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, importar todas',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        importarTodosMutation.mutate();
      }
    });
  };

  const handleImportarNovedad = (novedad: NovedadTango) => {
    Swal.fire({
      title: '¿Importar esta novedad?',
      html: `
        <p><strong>Código:</strong> ${novedad.codNovedad}</p>
        <p><strong>Descripción:</strong> ${novedad.descNovedad}</p>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, importar',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        importarUnaMutation.mutate(novedad.idNovedad);
      }
    });
  };

  // Verificar si una novedad de Tango ya está importada (por código)
  const estaImportado = (codNovedad: string): boolean => {
    return novedades?.some((n) => n.codNovedad === codNovedad) || false;
  };

  // Filtrar novedades locales por búsqueda
  const novedadesFiltradas = novedades?.filter((novedad) => {
    if (!searchTerm) return true;
    const search = searchTerm.toLowerCase();
    return (
      novedad.codNovedad.toLowerCase().includes(search) ||
      novedad.descNovedad.toLowerCase().includes(search)
    );
  });

  // Filtrar novedades de Tango por búsqueda
  const novedadesTangoFiltradas = novedadesTango?.filter((novedad) => {
    if (!searchTerm) return true;
    const search = searchTerm.toLowerCase();
    return (
      novedad.codNovedad.toLowerCase().includes(search) ||
      novedad.descNovedad.toLowerCase().includes(search)
    );
  });

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Novedades</h1>
        {isAdmin && activeTab === 'tango' && (
          <button
            className="btn btn-success"
            onClick={handleImportarTodos}
            disabled={importarTodosMutation.isPending}
          >
            {importarTodosMutation.isPending ? (
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
                <i className="bi bi-download me-2"></i>
                Importar Todas
              </>
            )}
          </button>
        )}
      </div>

      {/* Pestañas */}
      <ul className="nav nav-tabs mb-3">
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'locales' ? 'active' : ''}`}
            onClick={() => setActiveTab('locales')}
          >
            <i className="bi bi-database me-2"></i>
            Novedades Locales ({novedades?.length || 0})
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'tango' ? 'active' : ''}`}
            onClick={() => setActiveTab('tango')}
          >
            <i className="bi bi-cloud-download me-2"></i>
            Disponibles en Tango ({novedadesTango?.length || 0})
          </button>
        </li>
      </ul>

      {/* Búsqueda */}
      <div className="card mb-3">
        <div className="card-body">
          <div className="row">
            <div className="col-md-6">
              <label htmlFor="search" className="form-label">
                Buscar novedad:
              </label>
              <input
                id="search"
                type="text"
                className="form-control"
                placeholder="Buscar por código o descripción..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Contenido de las pestañas */}
      {activeTab === 'locales' ? (
        // Pestaña de novedades locales
        loadingLocales ? (
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
                      <th>Código</th>
                      <th>Descripción</th>
                      <th>Última Modificación</th>
                    </tr>
                  </thead>
                  <tbody>
                    {novedadesFiltradas?.map((novedad) => (
                      <tr key={novedad.idNovedad}>
                        <td>
                          <strong>{novedad.codNovedad}</strong>
                        </td>
                        <td>{novedad.descNovedad}</td>
                        <td>
                          {novedad.fechaModificacion
                            ? new Date(novedad.fechaModificacion).toLocaleDateString('es-AR')
                            : '-'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>

                {novedadesFiltradas?.length === 0 && (
                  <div className="alert alert-info">
                    {searchTerm
                      ? 'No se encontraron novedades que coincidan con la búsqueda'
                      : 'No hay novedades cargadas. Vaya a la pestaña "Disponibles en Tango" para importar.'}
                  </div>
                )}
              </div>

              {novedadesFiltradas && novedadesFiltradas.length > 0 && (
                <div className="mt-3">
                  <small className="text-muted">
                    Mostrando {novedadesFiltradas.length} de {novedades?.length} novedades
                  </small>
                </div>
              )}
            </div>
          </div>
        )
      ) : (
        // Pestaña de novedades de Tango
        loadingTango ? (
          <div className="text-center">
            <div className="spinner-border" role="status">
              <span className="visually-hidden">Cargando...</span>
            </div>
          </div>
        ) : (
          <div className="card">
            <div className="card-body">
              <div className="alert alert-info">
                <i className="bi bi-info-circle me-2"></i>
                <strong>Novedades disponibles en Tango.</strong> Haga clic en "Importar" para agregar
                una novedad específica a su base local.
              </div>

              <div className="table-responsive">
                <table className="table table-striped table-hover">
                  <thead>
                    <tr>
                      <th>Código</th>
                      <th>Descripción</th>
                      <th>ID Tango</th>
                      <th>Estado</th>
                      <th style={{ width: '150px' }}>Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {novedadesTangoFiltradas?.map((novedad) => {
                      const importado = estaImportado(novedad.codNovedad);
                      return (
                        <tr key={novedad.idNovedad}>
                          <td>
                            <strong>{novedad.codNovedad}</strong>
                          </td>
                          <td>{novedad.descNovedad}</td>
                          <td>
                            <span className="badge bg-secondary">{novedad.idNovedad}</span>
                          </td>
                          <td>
                            {importado ? (
                              <span className="badge bg-success">
                                <i className="bi bi-check-circle me-1"></i>
                                Importado
                              </span>
                            ) : (
                              <span className="badge bg-warning text-dark">
                                <i className="bi bi-exclamation-circle me-1"></i>
                                No importado
                              </span>
                            )}
                          </td>
                          <td>
                            {isAdmin && !importado && (
                              <button
                                className="btn btn-sm btn-primary"
                                onClick={() => handleImportarNovedad(novedad)}
                                disabled={importarUnaMutation.isPending}
                              >
                                {importarUnaMutation.isPending ? (
                                  <span
                                    className="spinner-border spinner-border-sm"
                                    role="status"
                                    aria-hidden="true"
                                  ></span>
                                ) : (
                                  <>
                                    <i className="bi bi-download me-1"></i>
                                    Importar
                                  </>
                                )}
                              </button>
                            )}
                            {importado && (
                              <span className="text-muted">
                                <i className="bi bi-check me-1"></i>
                                Ya importado
                              </span>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>

                {novedadesTangoFiltradas?.length === 0 && (
                  <div className="alert alert-info">
                    {searchTerm
                      ? 'No se encontraron novedades que coincidan con la búsqueda'
                      : 'No hay novedades disponibles en Tango'}
                  </div>
                )}
              </div>

              {novedadesTangoFiltradas && novedadesTangoFiltradas.length > 0 && (
                <div className="mt-3">
                  <small className="text-muted">
                    Mostrando {novedadesTangoFiltradas.length} de {novedadesTango?.length}{' '}
                    novedades disponibles
                  </small>
                </div>
              )}
            </div>
          </div>
        )
      )}
    </div>
  );
};
