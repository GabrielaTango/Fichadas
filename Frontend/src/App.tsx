import { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAuthStore } from './stores/authStore';
import { Layout } from './components/Layout';
import { Login } from './pages/Login';
import { Home } from './pages/Home';
import { Empleados } from './pages/Empleados';
import { Fichadas } from './pages/Fichadas';
import { ImportarFichadas } from './pages/ImportarFichadas';
import { Sectores } from './pages/Sectores';
import { Configuraciones } from './pages/Configuraciones';
import { Novedades } from './pages/Novedades';
import { Usuarios } from './pages/Usuarios';
import { CambiarPassword } from './pages/CambiarPassword';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function App() {
  const initialize = useAuthStore((state) => state.initialize);

  useEffect(() => {
    initialize();
  }, [initialize]);

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route
            path="/"
            element={
              <Layout>
                <Home />
              </Layout>
            }
          />
          <Route
            path="/empleados"
            element={
              <Layout>
                <Empleados />
              </Layout>
            }
          />
          <Route
            path="/fichadas"
            element={
              <Layout>
                <Fichadas />
              </Layout>
            }
          />
          <Route
            path="/importar-fichadas"
            element={
              <Layout>
                <ImportarFichadas />
              </Layout>
            }
          />
          <Route
            path="/sectores"
            element={
              <Layout>
                <Sectores />
              </Layout>
            }
          />
          <Route
            path="/configuraciones"
            element={
              <Layout>
                <Configuraciones />
              </Layout>
            }
          />
          <Route
            path="/novedades"
            element={
              <Layout>
                <Novedades />
              </Layout>
            }
          />
          <Route
            path="/usuarios"
            element={
              <Layout>
                <Usuarios />
              </Layout>
            }
          />
          <Route
            path="/cambiar-password"
            element={
              <Layout>
                <CambiarPassword />
              </Layout>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
