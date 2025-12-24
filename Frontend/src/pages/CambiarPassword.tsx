import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { authService } from '../services/authService';
import { useAuthStore } from '../stores/authStore';
import Swal from 'sweetalert2';
import { useNavigate } from 'react-router-dom';

export const CambiarPassword = () => {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const changePasswordMutation = useMutation({
    mutationFn: ({ userId, currentPassword, newPassword }: {
      userId: number;
      currentPassword: string;
      newPassword: string
    }) => authService.changePassword(userId, currentPassword, newPassword),
    onSuccess: () => {
      Swal.fire({
        title: '¡Éxito!',
        text: 'Contraseña actualizada correctamente',
        icon: 'success',
      }).then(() => {
        // Limpiar el formulario
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
        navigate('/');
      });
    },
    onError: (error: any) => {
      Swal.fire(
        'Error',
        error.response?.data?.message || 'No se pudo cambiar la contraseña',
        'error'
      );
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Validaciones
    if (!currentPassword || !newPassword || !confirmPassword) {
      Swal.fire('Error', 'Complete todos los campos', 'error');
      return;
    }

    if (newPassword.length < 4) {
      Swal.fire('Error', 'La nueva contraseña debe tener al menos 4 caracteres', 'error');
      return;
    }

    if (newPassword !== confirmPassword) {
      Swal.fire('Error', 'Las contraseñas no coinciden', 'error');
      return;
    }

    if (currentPassword === newPassword) {
      Swal.fire('Error', 'La nueva contraseña debe ser diferente a la actual', 'error');
      return;
    }

    if (!user?.idUsuario) {
      Swal.fire('Error', 'No se pudo identificar al usuario', 'error');
      return;
    }

    // Ejecutar cambio de contraseña
    changePasswordMutation.mutate({
      userId: user.idUsuario,
      currentPassword,
      newPassword,
    });
  };

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card">
            <div className="card-header">
              <h4 className="mb-0">
                <i className="bi bi-key me-2"></i>
                Cambiar Contraseña
              </h4>
            </div>
            <div className="card-body">
              <div className="alert alert-info">
                <i className="bi bi-info-circle me-2"></i>
                Usuario: <strong>{user?.usuario}</strong>
              </div>

              <form onSubmit={handleSubmit}>
                <div className="mb-3">
                  <label htmlFor="currentPassword" className="form-label">
                    Contraseña Actual *
                  </label>
                  <input
                    id="currentPassword"
                    type="password"
                    className="form-control"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                    required
                    autoComplete="current-password"
                  />
                </div>

                <div className="mb-3">
                  <label htmlFor="newPassword" className="form-label">
                    Nueva Contraseña *
                  </label>
                  <input
                    id="newPassword"
                    type="password"
                    className="form-control"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    required
                    minLength={4}
                    autoComplete="new-password"
                  />
                  <small className="form-text text-muted">
                    Mínimo 4 caracteres
                  </small>
                </div>

                <div className="mb-3">
                  <label htmlFor="confirmPassword" className="form-label">
                    Confirmar Nueva Contraseña *
                  </label>
                  <input
                    id="confirmPassword"
                    type="password"
                    className="form-control"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    required
                    minLength={4}
                    autoComplete="new-password"
                  />
                  {newPassword && confirmPassword && newPassword !== confirmPassword && (
                    <small className="text-danger">
                      Las contraseñas no coinciden
                    </small>
                  )}
                </div>

                <div className="d-grid gap-2">
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={changePasswordMutation.isPending}
                  >
                    {changePasswordMutation.isPending ? (
                      <>
                        <span
                          className="spinner-border spinner-border-sm me-2"
                          role="status"
                          aria-hidden="true"
                        ></span>
                        Cambiando contraseña...
                      </>
                    ) : (
                      <>
                        <i className="bi bi-check-circle me-2"></i>
                        Cambiar Contraseña
                      </>
                    )}
                  </button>
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => navigate('/')}
                  >
                    Cancelar
                  </button>
                </div>
              </form>
            </div>
          </div>

          <div className="mt-3">
            <div className="alert alert-warning">
              <strong>Importante:</strong>
              <ul className="mb-0 mt-2">
                <li>La nueva contraseña debe tener al menos 4 caracteres</li>
                <li>Debe ser diferente a la contraseña actual</li>
                <li>Asegúrese de recordar su nueva contraseña</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
