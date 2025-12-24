import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { fichadasService } from '../services/fichadasService';

export const ImportarFichadas = () => {
  const [archivo, setArchivo] = useState<File | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [resultado, setResultado] = useState<{
    message: string;
    fichadasImportadas: number;
    fichadasIgnoradas: number;
    errores?: string[];
  } | null>(null);

  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (file: File) => fichadasService.importar(file),
    onSuccess: (data) => {
      setResultado(data);
      setArchivo(null);
      // Invalidar la cache de fichadas para que se recarguen
      queryClient.invalidateQueries({ queryKey: ['fichadas'] });
    },
    onError: (error: any) => {
      alert('Error al importar fichadas: ' + (error.response?.data?.message || error.message));
    },
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setArchivo(e.target.files[0]);
      setResultado(null);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      const file = e.dataTransfer.files[0];

      // Validar extensión
      if (file.name.endsWith('.xlsx') || file.name.endsWith('.xls')) {
        setArchivo(file);
        setResultado(null);
      } else {
        alert('Por favor, seleccione un archivo Excel (.xlsx o .xls)');
      }
    }
  };

  const handleImportar = () => {
    if (!archivo) {
      alert('Por favor, seleccione un archivo');
      return;
    }

    mutation.mutate(archivo);
  };

  const handleLimpiar = () => {
    setArchivo(null);
    setResultado(null);
  };

  return (
    <div className="container">

      <h1 className="mb-2">Importar Fichadas desde Excel</h1>
      <p className="text-muted mb-4">Importe fichadas masivamente desde un archivo Excel</p>

      {resultado && (
        <div className={`alert ${resultado.errores && resultado.errores.length > 0 ? 'alert-warning' : 'alert-success'}`}>
          <h5 className="alert-heading">{resultado.message}</h5>
          <div className="d-flex gap-4 my-3">
            <div>
              <strong>Importadas:</strong> <span className="badge bg-success fs-6">{resultado.fichadasImportadas}</span>
            </div>
            <div>
              <strong>Ignoradas:</strong> <span className="badge bg-warning fs-6">{resultado.fichadasIgnoradas}</span>
            </div>
          </div>

          {resultado.errores && resultado.errores.length > 0 && (
            <>
              <hr />
              <h6>Advertencias y errores:</h6>
              <div className="list-group">
                {resultado.errores.slice(0, 10).map((error, index) => (
                  <div key={index} className="list-group-item list-group-item-warning">
                    {error}
                  </div>
                ))}
                {resultado.errores.length > 10 && (
                  <div className="list-group-item list-group-item-warning">
                    ... y {resultado.errores.length - 10} errores más
                  </div>
                )}
              </div>
            </>
          )}
        </div>
      )}

      <div className="alert alert-info mb-4">
        <h5 className="alert-heading">Formato del archivo Excel</h5>
        <ul className="mb-2">
          <li><strong>Columna A:</strong> Legajo del empleado (idPersona)</li>
          <li><strong>Columna G:</strong> Fecha en formato yyyyMMdd</li>
          <li><strong>Columna H:</strong> Fichadas (horas separadas por ; )</li>
        </ul>
        <hr />
        <p className="mb-0"><strong>Ejemplo de fichadas:</strong></p>
        <ul className="mb-0">
          <li>"08:00;17:00" → Entrada 08:00, Salida 17:00 mismo día</li>
          <li>"08:00;17+30" → Entrada 08:00, Salida 17:30 al día siguiente</li>
        </ul>
      </div>

      <div
        className={`card mb-4 ${isDragging ? 'border-primary bg-light' : ''}`}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        style={{ borderStyle: 'dashed', borderWidth: '2px', transition: 'all 0.3s' }}
      >
        <div className="card-body text-center py-5">
          {!archivo ? (
            <>
              <div style={{ fontSize: '4rem' }}>📁</div>
              <p className="lead">Arrastre el archivo Excel aquí</p>
              <p className="text-muted">o</p>
              <label className="btn btn-primary">
                Seleccionar archivo
                <input
                  type="file"
                  accept=".xlsx,.xls"
                  onChange={handleFileChange}
                  style={{ display: 'none' }}
                />
              </label>
              <p className="text-muted mt-3 mb-0"><small>Formatos soportados: .xlsx, .xls</small></p>
            </>
          ) : (
            <div className="d-flex align-items-center justify-content-center gap-3">
              <div style={{ fontSize: '3rem' }}>📄</div>
              <div className="text-start">
                <h6 className="mb-0">{archivo.name}</h6>
                <small className="text-muted">{(archivo.size / 1024).toFixed(2)} KB</small>
              </div>
              <button className="btn btn-danger btn-sm ms-auto" onClick={handleLimpiar}>
                ✕
              </button>
            </div>
          )}
        </div>
      </div>

      {archivo && (
        <div className="d-flex gap-2 mb-4">
          <button
            className="btn btn-success"
            onClick={handleImportar}
            disabled={mutation.isPending}
          >
            {mutation.isPending ? 'Importando...' : 'Importar Fichadas'}
          </button>
          <button className="btn btn-secondary" onClick={handleLimpiar}>
            Cancelar
          </button>
        </div>
      )}
    </div>
  );
};
