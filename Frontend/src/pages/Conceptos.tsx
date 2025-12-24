import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import Swal from 'sweetalert2';
import { conceptosService } from '../services/conceptosService';
import { useAuthStore } from '../stores/authStore';

type ConceptoTango = {
  idConcepto: number;
  nroConcepto: number;
  descConcepto: string;
};

export const Conceptos = () => {
  const queryClient = useQueryClient();
  const user = useAuthStore((state) => state.user);
  const isAdmin = user?.esAdmin || false;
  const [searchTerm, setSearchTerm] = useState('');
  const [activeTab, setActiveTab] = useState<'locales' | 'tango'>('locales');

  // Query para conceptos locales
  const { data: conceptos, isLoading: loadingLocales } = useQuery({
    queryKey: ['conceptos'],
    queryFn: conceptosService.getAll,
  });

  // Query para conceptos disponibles en Tango
  const { data: conceptosTango, isLoading: loadingTango, refetch: refetchTango } = useQuery({
    queryKey: ['conceptos-tango'],
    queryFn: conceptosService.getDisponiblesTango,
    enabled: activeTab === 'tango',
  });

  const importarTodosMutation = useMutation({
    mutationFn: () => conceptosService.importarDesdeTango(),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['conceptos'] });
      queryClient.invalidateQueries({ queryKey: ['conceptos-tango'] });

      let message = data.message;
      if (data.errores && data.errores.length > 0) {
        message += '\n\nErrores:\n' + data.errores.join('\n');
        Swal.fire('Importación Completada con Advertencias', message, 'warning');
      } else {
        Swal.fire('¡Éxito!', message, 'success');
      }
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al importar conceptos desde Tango', 'error');
    },
  });

  const importarUnoMutation = useMutation({
    mutationFn: (idConceptoTango: number) => conceptosService.importarConceptoEspecifico(idConceptoTango),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conceptos'] });
      refetchTango();
      Swal.fire('¡Éxito!', 'Concepto importado correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al importar concepto', 'error');
    },
  });

  const handleImportarTodos = () => {
    Swal.fire({
      title: '¿Importar TODOS los conceptos desde Tango?',
      html: `
        <p>Esta acción importará todos los conceptos desde la base de datos de Tango.</p>
        <ul style="text-align: left; margin-top: 10px;">
          <li>Los conceptos <strong>nuevos</strong> se crearán automáticamente</li>
          <li>Los conceptos <strong>existentes</strong> se actualizarán si cambió su descripción</li>
          <li>No se borrarán conceptos existentes</li>
        </ul>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, importar todos',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        importarTodosMutation.mutate();
      }
    });
  };

  const handleImportarConcepto = (concepto: ConceptoTango) => {
    Swal.fire({
      title: '¿Importar este concepto?',
      html: `
        <p><strong>Nro:</strong> ${concepto.nroConcepto}</p>
        <p><strong>Descripción:</strong> ${concepto.descConcepto}</p>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, importar',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        importarUnoMutation.mutate(concepto.idConcepto);
      }
    });
  };

  // Verificar si un concepto de Tango ya está importado
  const estaImportado = (idConceptoTango: number): boolean => {
    return conceptos?.some((c) => c.idConceptoTango === idConceptoTango) || false;
  };

  // Filtrar conceptos locales por búsqueda
  const conceptosFiltrados = conceptos?.filter((concepto) => {
    if (!searchTerm) return true;
    const search = searchTerm.toLowerCase();
    return (
      concepto.nroConcepto.toString().includes(search) ||
      concepto.descConcepto.toLowerCase().includes(search)
    );
  });

  // Filtrar conceptos de Tango por búsqueda
  const conceptosTangoFiltrados = conceptosTango?.filter((concepto) => {
    if (!searchTerm) return true;
    const search = searchTerm.toLowerCase();
    return (
      concepto.nroConcepto.toString().includes(search) ||
      concepto.descConcepto.toLowerCase().includes(search)
    );
  });

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Conceptos de Novedades</h1>
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
                Importar Todos
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
            Conceptos Locales ({conceptos?.length || 0})
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'tango' ? 'active' : ''}`}
            onClick={() => setActiveTab('tango')}
          >
            <i className="bi bi-cloud-download me-2"></i>
            Disponibles en Tango ({conceptosTango?.length || 0})
          </button>
        </li>
      </ul>

      {/* Búsqueda */}
      <div className="card mb-3">
        <div className="card-body">
          <div className="row">
            <div className="col-md-6">
              <label htmlFor="search" className="form-label">
                Buscar concepto:
              </label>
              <input
                id="search"
                type="text"
                className="form-control"
                placeholder="Buscar por número o descripción..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Contenido de las pestañas */}
      {activeTab === 'locales' ? (
        // Pestaña de conceptos locales
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
                      <th>Nro. Concepto</th>
                      <th>Descripción</th>
                      <th>ID Tango</th>
                      <th>Última Modificación</th>
                    </tr>
                  </thead>
                  <tbody>
                    {conceptosFiltrados?.map((concepto) => (
                      <tr key={concepto.idConcepto}>
                        <td>
                          <strong>{concepto.nroConcepto}</strong>
                        </td>
                        <td>{concepto.descConcepto}</td>
                        <td>
                          <span className="badge bg-secondary">{concepto.idConceptoTango}</span>
                        </td>
                        <td>
                          {concepto.fechaModificacion
                            ? new Date(concepto.fechaModificacion).toLocaleDateString('es-AR')
                            : '-'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>

                {conceptosFiltrados?.length === 0 && (
                  <div className="alert alert-info">
                    {searchTerm
                      ? 'No se encontraron conceptos que coincidan con la búsqueda'
                      : 'No hay conceptos cargados. Vaya a la pestaña "Disponibles en Tango" para importar.'}
                  </div>
                )}
              </div>

              {conceptosFiltrados && conceptosFiltrados.length > 0 && (
                <div className="mt-3">
                  <small className="text-muted">
                    Mostrando {conceptosFiltrados.length} de {conceptos?.length} conceptos
                  </small>
                </div>
              )}
            </div>
          </div>
        )
      ) : (
        // Pestaña de conceptos de Tango
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
                <strong>Conceptos disponibles en Tango.</strong> Haga clic en "Importar" para agregar
                un concepto específico a su base local.
              </div>

              <div className="table-responsive">
                <table className="table table-striped table-hover">
                  <thead>
                    <tr>
                      <th>Nro. Concepto</th>
                      <th>Descripción</th>
                      <th>ID Tango</th>
                      <th>Estado</th>
                      <th style={{ width: '150px' }}>Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {conceptosTangoFiltrados?.map((concepto) => {
                      const importado = estaImportado(concepto.idConcepto);
                      return (
                        <tr key={concepto.idConcepto}>
                          <td>
                            <strong>{concepto.nroConcepto}</strong>
                          </td>
                          <td>{concepto.descConcepto}</td>
                          <td>
                            <span className="badge bg-secondary">{concepto.idConcepto}</span>
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
                                onClick={() => handleImportarConcepto(concepto)}
                                disabled={importarUnoMutation.isPending}
                              >
                                {importarUnoMutation.isPending ? (
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

                {conceptosTangoFiltrados?.length === 0 && (
                  <div className="alert alert-info">
                    {searchTerm
                      ? 'No se encontraron conceptos que coincidan con la búsqueda'
                      : 'No hay conceptos disponibles en Tango'}
                  </div>
                )}
              </div>

              {conceptosTangoFiltrados && conceptosTangoFiltrados.length > 0 && (
                <div className="mt-3">
                  <small className="text-muted">
                    Mostrando {conceptosTangoFiltrados.length} de {conceptosTango?.length}{' '}
                    conceptos disponibles
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
