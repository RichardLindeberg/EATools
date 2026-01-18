import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClientProvider } from '@tanstack/react-query'
import './index.css'
import App from './App.tsx'
import queryClient from './api/queryClient'

console.log('main.tsx loading...')

const rootElement = document.getElementById('root')
console.log('Root element:', rootElement)

if (rootElement) {
  try {
    console.log('Creating React root...')
    const root = createRoot(rootElement)
    console.log('Rendering app...')
    root.render(
      <StrictMode>
        <QueryClientProvider client={queryClient}>
          <App />
        </QueryClientProvider>
      </StrictMode>,
    )
    console.log('App rendered successfully')
  } catch (error) {
    console.error('Error rendering app:', error)
  }
} else {
  console.error('Root element not found!')
}
