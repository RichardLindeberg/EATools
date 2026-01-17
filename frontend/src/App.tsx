import { Routes, Route, Navigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { checkHealth } from './api/client'
import './App.css'

// Placeholder pages - will be implemented in subsequent items
const Dashboard = () => <div className="page"><h1>Dashboard</h1><p>Coming soon...</p></div>
const Login = () => <div className="page"><h1>Login</h1><p>Coming soon...</p></div>
const NotFound = () => <div className="page"><h1>404 - Page Not Found</h1></div>

function App() {
  // Check API health on mount
  const { data: health, isLoading, error } = useQuery({
    queryKey: ['health'],
    queryFn: checkHealth,
  })

  if (isLoading) {
    return <div className="loading">Loading...</div>
  }

  return (
    <div className="app">
      {error && (
        <div className="error-banner">
          ⚠️ Backend API connection failed. Please ensure the server is running.
        </div>
      )}
      {health && <div className="success-banner">✓ Connected to API</div>}
      
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/login" element={<Login />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </div>
  )
}

export default App
