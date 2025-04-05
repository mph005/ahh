import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link, Navigate } from 'react-router-dom';
import AppointmentList from './components/AppointmentList';
import TherapistList from './components/TherapistList';
import ClientList from './components/ClientList';
import Login from './components/Login';
import AdminDashboard from './components/AdminDashboard';
import RegisterPage from './components/RegisterPage';
import './App.css';
import { useAuth } from './context/AuthContext';

// Helper component for protected routes
function ProtectedRoute({ children, requiredRole }) {
  const { isAuthenticated, user, isLoading } = useAuth();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isAuthenticated) {
    // Redirect to login if not authenticated
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && (!user?.roles || !user.roles.includes(requiredRole))) {
    // Redirect or show unauthorized message if role requirement not met
    console.warn(`User does not have required role: ${requiredRole}`);
    return <Navigate to="/" replace />;
  }

  return children;
}

function App() {
  const { isAuthenticated, user, logout, authError, isLoading } = useAuth();

  return (
    <Router>
      <div className="App">
        <header className="App-header">
          <h1>Massage Booking System</h1>
          <nav>
            <ul>
              <li><Link to="/">Home (Appointments)</Link></li>
              {/* Conditional Admin links */}
              {isAuthenticated && user?.roles?.includes('Admin') && (
                <>
                  <li><Link to="/admin">Admin Dashboard</Link></li>
                  <li><Link to="/clients">Manage Clients</Link></li>
                  <li><Link to="/therapists">Manage Therapists</Link></li>
                </>
              )}
              {/* Add other links as needed */}
            </ul>
          </nav>
          {/* Auth status display */}
          {isLoading ? (
            <span>Loading user...</span>
          ) : isAuthenticated ? (
            <div>
              <span>Welcome, {user?.email}! ({user?.roles?.join(', ')})</span>
              <button onClick={logout} style={{ marginLeft: '10px' }}>Logout</button>
            </div>
          ) : (
             <Link to="/login">Login</Link>
          )}
        </header>
        <main className="App-content">
          {authError && <p className="error-message">Auth Error: {authError}</p>}
          <Routes>
             {/* Public or common routes */}
             <Route path="/login" element={!isAuthenticated && !isLoading ? <Login /> : <Navigate to="/" replace />} />
             <Route path="/register" element={!isAuthenticated && !isLoading ? <RegisterPage /> : <Navigate to="/" replace />} />

             {/* Protected Routes */}
             <Route
               path="/"
               element={
                 <ProtectedRoute>
                   <AppointmentList />
                 </ProtectedRoute>
               }
             />
             <Route
               path="/clients"
               element={
                 <ProtectedRoute requiredRole="Admin">
                   <ClientList />
                 </ProtectedRoute>
               }
             />
             <Route
               path="/therapists"
               element={
                 <ProtectedRoute requiredRole="Admin">
                   <TherapistList />
                 </ProtectedRoute>
               }
             />

             {/* Admin Route */}
             <Route
               path="/admin"
               element={
                 <ProtectedRoute requiredRole="Admin">
                   <AdminDashboard />
                 </ProtectedRoute>
               }
             />

            {/* Fallback for unmatched routes */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
        <footer className="App-footer">
          <p>&copy; 2024 Massage Booking System</p>
        </footer>
      </div>
    </Router>
  );
}

export default App; 