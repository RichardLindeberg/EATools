import { RouterProvider } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import { SessionTimeout } from './components/auth/SessionTimeout'
import { router } from './router'
import './App.css'

function App() {
  return (
    <AuthProvider>
      <SessionTimeout />
      <RouterProvider router={router} />
    </AuthProvider>
  )
}

export default App
