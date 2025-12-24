import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usuariosService } from '../services/usuariosService';
import Swal from 'sweetalert2';
import type { Usuario, UsuarioFormData } from '../types';

export const Usuarios = () => {
  const [showModal, setShowModal] = useState(false);
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [editingUsuario, setEditingUsuario] = useState<Usuario | null>(null);
  const [selectedUsuarioId, setSelectedUsuarioId] = useState<number | null>(null);
  const [newPassword, setNewPassword] = useState('');
  const [formData, setFormData] = useState<UsuarioFormData>({
    usuario: '',
    password: '',
    mail: '',
    esAdmin: false,
  });

  const queryClient = useQueryClient();

  // Query para obtener usuarios
  const { data: usuarios, isLoading } = useQuery({
    queryKey: ['usuarios'],
    queryFn: usuariosService.getAll,
  });

  // Mutation para crear usuario
  const createMutation = useMutation({
    mutationFn: (data: UsuarioFormData) => usuariosService.create(data),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['usuarios'] });
      setShowModal(false);
      resetForm();
      Swal.fire('¡Éxito!', 'Usuario creado correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al crear usuario', 'error');
    },
  });

  // Mutation para actualizar usuario
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Omit<UsuarioFormData, 'password'> }) =>
      usuariosService.update(id, data),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['usuarios'] });
      setShowModal(false);
      resetForm();
      Swal.fire('¡Éxito!', 'Usuario actualizado correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al actualizar usuario', 'error');
    },
  });

  // Mutation para eliminar usuario
  const deleteMutation = useMutation({
    mutationFn: (id: number) => usuariosService.delete(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['usuarios'] });
      Swal.fire('¡Éxito!', 'Usuario eliminado correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al eliminar usuario', 'error');
    },
  });

  // Mutation para cambiar contraseña
  const changePasswordMutation = useMutation({
    mutationFn: ({ id, password }: { id: number; password: string }) =>
      usuariosService.changePassword(id, password),
    onSuccess: async () => {
      setShowPasswordModal(false);
      setNewPassword('');
      setSelectedUsuarioId(null);
      Swal.fire('¡Éxito!', 'Contraseña actualizada correctamente', 'success');
    },
    onError: (error: any) => {
      Swal.fire('Error', error.response?.data?.message || 'Error al cambiar contraseña', 'error');
    },
  });

  const resetForm = () => {
    setFormData({
      usuario: '',
      password: '',
      mail: '',
      esAdmin: false,
    });
    setEditingUsuario(null);
  };

  const handleOpenModal = (usuario?: Usuario) => {
    if (usuario) {
      // Editar
      setEditingUsuario(usuario);
      setFormData({
        usuario: usuario.usuario,
        mail: usuario.mail,
        esAdmin: usuario.esAdmin,
      });
    } else {
      // Crear nuevo
      resetForm();
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    resetForm();
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.usuario || !formData.mail) {
      Swal.fire('Error', 'Complete los campos obligatorios', 'error');
      return;
    }

    if (!editingUsuario && !formData.password) {
      Swal.fire('Error', 'La contraseña es obligatoria para crear un usuario', 'error');
      return;
    }

    if (editingUsuario) {
      // Actualizar (sin contraseña)
      const { password, ...updateData } = formData;
      updateMutation.mutate({ id: editingUsuario.idUsuario, data: updateData });
    } else {
      // Crear nuevo (con contraseña)
      createMutation.mutate(formData);
    }
  };

  const handleDelete = (usuario: Usuario) => {
    if (usuario.usuario.toLowerCase() === 'admin') {
      Swal.fire('Error', 'No se puede eliminar el usuario admin', 'error');
      return;
    }

    Swal.fire({
      title: '¿Está seguro?',
      text: `¿Desea eliminar el usuario "${usuario.usuario}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        deleteMutation.mutate(usuario.idUsuario);
      }
    });
  };

  const handleOpenPasswordModal = (usuario: Usuario) => {
    setSelectedUsuarioId(usuario.idUsuario);
    setNewPassword('');
    setShowPasswordModal(true);
  };

  const handleChangePassword = (e: React.FormEvent) => {
    e.preventDefault();

    if (!newPassword || newPassword.length < 4) {
      Swal.fire('Error', 'La contraseña debe tener al menos 4 caracteres', 'error');
      return;
    }

    if (selectedUsuarioId) {
      changePasswordMutation.mutate({ id: selectedUsuarioId, password: newPassword });
    }
  };

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Gestión de Usuarios</h1>
        <button className="btn btn-primary" onClick={() => handleOpenModal()}>
          + Nuevo Usuario
        </button>
      </div>

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
                    <th>ID</th>
                    <th>Usuario</th>
                    <th>Email</th>
                    <th>Rol</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {usuarios?.map((usuario) => (
                    <tr key={usuario.idUsuario}>
                      <td>{usuario.idUsuario}</td>
                      <td>
                        <strong>{usuario.usuario}</strong>
                      </td>
                      <td>{usuario.mail}</td>
                      <td>
                        {usuario.esAdmin ? (
                          <span className="badge bg-danger">Administrador</span>
                        ) : (
                          <span className="badge bg-secondary">Usuario</span>
                        )}
                      </td>
                      <td>
                        <button
                          className="btn btn-sm btn-info me-1"
                          onClick={() => handleOpenPasswordModal(usuario)}
                          title="Cambiar contraseña"
                        >
                          <i className="bi bi-key"></i>
                        </button>
                        <button
                          className="btn btn-sm btn-warning me-1"
                          onClick={() => handleOpenModal(usuario)}
                          title="Editar"
                        >
                          <i className="bi bi-pencil"></i>
                        </button>
                        {usuario.usuario.toLowerCase() !== 'admin' && (
                          <button
                            className="btn btn-sm btn-danger"
                            onClick={() => handleDelete(usuario)}
                            title="Eliminar"
                          >
                            <i className="bi bi-trash"></i>
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {usuarios?.length === 0 && (
                <div className="alert alert-info">No hay usuarios registrados</div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Modal para crear/editar usuario */}
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
                  {editingUsuario ? 'Editar Usuario' : 'Nuevo Usuario'}
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
                    <label htmlFor="usuario" className="form-label">
                      Nombre de Usuario *
                    </label>
                    <input
                      id="usuario"
                      type="text"
                      className="form-control"
                      value={formData.usuario}
                      onChange={(e) => setFormData({ ...formData, usuario: e.target.value })}
                      required
                    />
                  </div>

                  {!editingUsuario && (
                    <div className="mb-3">
                      <label htmlFor="password" className="form-label">
                        Contraseña *
                      </label>
                      <input
                        id="password"
                        type="password"
                        className="form-control"
                        value={formData.password || ''}
                        onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                        required={!editingUsuario}
                        minLength={4}
                      />
                      <small className="form-text text-muted">
                        Mínimo 4 caracteres
                      </small>
                    </div>
                  )}

                  {editingUsuario && (
                    <div className="alert alert-info">
                      <i className="bi bi-info-circle me-2"></i>
                      Para cambiar la contraseña, use el botón de "Cambiar contraseña" en la tabla
                    </div>
                  )}

                  <div className="mb-3">
                    <label htmlFor="mail" className="form-label">
                      Email *
                    </label>
                    <input
                      id="mail"
                      type="email"
                      className="form-control"
                      value={formData.mail}
                      onChange={(e) => setFormData({ ...formData, mail: e.target.value })}
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <div className="form-check">
                      <input
                        id="esAdmin"
                        type="checkbox"
                        className="form-check-input"
                        checked={formData.esAdmin}
                        onChange={(e) => setFormData({ ...formData, esAdmin: e.target.checked })}
                      />
                      <label htmlFor="esAdmin" className="form-check-label">
                        Es Administrador
                      </label>
                    </div>
                    <small className="form-text text-muted">
                      Los administradores tienen acceso completo al sistema
                    </small>
                  </div>
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

      {/* Modal para cambiar contraseña */}
      {showPasswordModal && (
        <div
          className="modal show d-block"
          tabIndex={-1}
          style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
        >
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Cambiar Contraseña</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowPasswordModal(false)}
                ></button>
              </div>
              <form onSubmit={handleChangePassword}>
                <div className="modal-body">
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
                    />
                    <small className="form-text text-muted">
                      Mínimo 4 caracteres
                    </small>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setShowPasswordModal(false)}
                  >
                    Cancelar
                  </button>
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
                        Guardando...
                      </>
                    ) : (
                      'Cambiar Contraseña'
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
