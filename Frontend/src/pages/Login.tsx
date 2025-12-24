import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { authService } from '../services/authService';
import { useAuthStore } from '../stores/authStore';
import type { LoginRequest } from '../types';

export const Login = () => {
  const navigate = useNavigate();
  const setUser = useAuthStore((state) => state.setUser);
  const [error, setError] = useState<string>('');
  const [loading, setLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginRequest>();

  const onSubmit = async (data: LoginRequest) => {
    setLoading(true);
    setError('');

    try {
      const response = await authService.login(data);
      setUser(response);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Usuario o contraseña incorrectos');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="d-flex align-items-center justify-content-center min-vh-100 bg-light">
      <div className="card shadow" style={{ width: '400px' }}>
        <div className="card-body p-4">
          <h1 className="card-title text-center mb-2">Sistema de Fichadas</h1>
          <h5 className="card-subtitle text-center text-muted mb-4">Colombraro</h5>

          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="mb-3">
              <label htmlFor="usuario" className="form-label">Usuario</label>
              <input
                id="usuario"
                type="text"
                {...register('usuario', { required: 'El usuario es requerido' })}
                className={`form-control ${errors.usuario ? 'is-invalid' : ''}`}
              />
              {errors.usuario && (
                <div className="invalid-feedback">{errors.usuario.message}</div>
              )}
            </div>

            <div className="mb-3">
              <label htmlFor="password" className="form-label">Contraseña</label>
              <input
                id="password"
                type="password"
                {...register('password', { required: 'La contraseña es requerida' })}
                className={`form-control ${errors.password ? 'is-invalid' : ''}`}
              />
              {errors.password && (
                <div className="invalid-feedback">{errors.password.message}</div>
              )}
            </div>

            {error && <div className="alert alert-danger">{error}</div>}

            <button type="submit" disabled={loading} className="btn btn-primary w-100">
              {loading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};
