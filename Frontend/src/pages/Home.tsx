import { useAuthStore } from '../stores/authStore';

export const Home = () => {
  const user = useAuthStore((state) => state.user);

  return (
    <div className="container">
      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <h1 className="card-title">Bienvenido al Sistema de Fichadas</h1>
          <p className="lead">
            Hola, <strong>{user?.usuario}</strong>
          </p>
        </div>
      </div>

      <div className="row g-3 mb-4">
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-body">
              <h5 className="card-title">Empleados</h5>
              <p className="card-text">Gestiona la información de los empleados y sus sectores</p>
            </div>
          </div>
        </div>

        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-body">
              <h5 className="card-title">Fichadas</h5>
              <p className="card-text">Registra y consulta las fichadas de entrada y salida</p>
            </div>
          </div>
        </div>

        {user?.esAdmin && (
          <>
            <div className="col-md-6">
              <div className="card h-100">
                <div className="card-body">
                  <h5 className="card-title">Sectores</h5>
                  <p className="card-text">Administra los sectores de la empresa</p>
                </div>
              </div>
            </div>

            <div className="col-md-6">
              <div className="card h-100">
                <div className="card-body">
                  <h5 className="card-title">Horarios</h5>
                  <p className="card-text">Configura los horarios de turno por sector</p>
                </div>
              </div>
            </div>
          </>
        )}
      </div>

      <div className="card">
        <div className="card-body">
          <h5 className="card-title">Funcionalidades del Sistema</h5>
          <ul className="list-group list-group-flush">
            <li className="list-group-item">Importación de fichadas desde Excel</li>
            <li className="list-group-item">Cálculo automático de horas normales, extras y adicionales</li>
            <li className="list-group-item">Gestión de llegadas tarde y ausencias</li>
            <li className="list-group-item">Registro de novedades y licencias</li>
            <li className="list-group-item">Generación de reportes para RRHH</li>
          </ul>
        </div>
      </div>
    </div>
  );
};
