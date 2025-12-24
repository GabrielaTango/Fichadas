import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

export const Navbar = () => {
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
      <div className="container">
        <Link className="navbar-brand" to="/">
          Fichadas App
        </Link>

        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarContent"
          aria-controls="navbarContent"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        <div className="collapse navbar-collapse" id="navbarContent">
          <ul className="navbar-nav me-auto mb-2 mb-lg-0">
            <li className="nav-item">
              <Link className="nav-link" to="/empleados">Empleados</Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/fichadas">Fichadas</Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/novedades">Novedades</Link>
            </li>
            {user?.esAdmin && (
              <>
                <li className="nav-item">
                  <Link className="nav-link" to="/sectores">Sectores</Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/configuraciones">Configuraciones</Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/usuarios">Usuarios</Link>
                </li>
              </>
            )}
          </ul>

          <div className="btn-group">
            <button className="btn btn-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
              <i className="bi bi-person-circle me-1"></i>
              {user?.usuario}
            </button>

            <ul className="dropdown-menu dropdown-menu-end">
              <li>
                <Link className="dropdown-item" to="/cambiar-password">
                  <i className="bi bi-key me-2"></i>
                  Cambiar Contraseña
                </Link>
              </li>
              <li><hr className="dropdown-divider" /></li>
              <li>
                <button className="dropdown-item" type="button" onClick={handleLogout}>
                  <i className="bi bi-box-arrow-right me-2"></i>
                  Salir
                </button>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </nav>
  );
};
