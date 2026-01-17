import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios'
import * as tokenManager from '../utils/tokenManager'

// API client configuration
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8000/api'

// Track if token refresh is in progress
let isRefreshing = false
let failedQueue: Array<{
  resolve: (token: string) => void
  reject: (error: any) => void
}> = []

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else if (token) {
      prom.resolve(token)
    }
  })

  failedQueue = []
}

// Create Axios instance
export const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor - add auth token if available
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = tokenManager.getAccessToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor - handle errors and token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // Handle 401 Unauthorized errors
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      // Avoid retry loop for auth endpoints
      if (originalRequest.url?.includes('/auth/')) {
        return Promise.reject(error)
      }

      if (isRefreshing) {
        // Token refresh already in progress, queue this request
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((token) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`
            }
            return apiClient(originalRequest)
          })
          .catch((err) => {
            return Promise.reject(err)
          })
      }

      originalRequest._retry = true
      isRefreshing = true

      const refreshToken = tokenManager.getRefreshToken()

      if (!refreshToken) {
        // No refresh token, redirect to login
        tokenManager.clearTokens()
        window.location.href = '/auth/login'
        return Promise.reject(error)
      }

      try {
        // Attempt to refresh token
        const response = await axios.post<{ accessToken: string }>(
          `${API_BASE_URL}/auth/refresh`,
          { refreshToken }
        )

        const newAccessToken = response.data.accessToken
        tokenManager.setAccessToken(newAccessToken)

        // Process queued requests
        processQueue(null, newAccessToken)

        // Retry original request
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`
        }
        return apiClient(originalRequest)
      } catch (refreshError) {
        // Token refresh failed, clear tokens and redirect to login
        processQueue(refreshError, null)
        tokenManager.clearTokens()
        window.location.href = '/auth/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  }
)

// Health check endpoint
export const checkHealth = async () => {
  try {
    const response = await apiClient.get('/health')
    return response.data
  } catch (error) {
    console.error('Health check failed:', error)
    throw error
  }
}

export default apiClient
