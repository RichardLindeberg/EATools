/**
 * Login Page Component
 * Provides email/password authentication form
 */

import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { useAuth } from '../../hooks/useAuth';
import { Button } from '../../components/Button/Button';
import { TextInput } from '../../components/Form/TextInput';
import { FormField } from '../../components/Form/FormField';
import { Alert } from '../../components/Alert/Alert';
import { Card } from '../../components/Card/Card';
import './LoginPage.css';

interface LoginFormData {
  email: string;
  password: string;
  rememberMe: boolean;
}

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, isAuthenticated, isLoading: authLoading, error: authError } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [loginError, setLoginError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
  });

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated && !authLoading) {
      const returnUrl = (location.state as any)?.from || '/';
      navigate(returnUrl, { replace: true });
    }
  }, [isAuthenticated, authLoading, navigate, location]);

  const onSubmit = async (data: LoginFormData) => {
    setIsSubmitting(true);
    setLoginError(null);

    try {
      await login({
        email: data.email,
        password: data.password,
        rememberMe: data.rememberMe,
      });

      // Redirect handled by useEffect above
    } catch (error: any) {
      setLoginError(error.message || 'Login failed. Please check your credentials.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (authLoading) {
    return (
      <div className="login-page">
        <div className="login-container">
          <Card>
            <div className="login-loading">Loading...</div>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="login-page">
      <div className="login-container">
        <Card>
          <div className="login-header">
            <h1 className="login-title">EATool</h1>
            <p className="login-subtitle">Sign in to your account</p>
          </div>

          {(loginError || authError) && (
            <Alert variant="danger" className="login-alert">
              {loginError || authError}
            </Alert>
          )}

          <form onSubmit={handleSubmit(onSubmit)} className="login-form" noValidate>
            <FormField
              label="Email"
              error={errors.email?.message}
              required
            >
              <TextInput
                {...register('email', {
                  required: 'Email is required',
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: 'Invalid email address',
                  },
                })}
                type="email"
                placeholder="you@example.com"
                error={!!errors.email}
                autoComplete="email"
                autoFocus
              />
            </FormField>

            <FormField
              label="Password"
              error={errors.password?.message}
              required
            >
              <TextInput
                {...register('password', {
                  required: 'Password is required',
                  minLength: {
                    value: 8,
                    message: 'Password must be at least 8 characters',
                  },
                })}
                type="password"
                placeholder="Enter your password"
                error={!!errors.password}
                autoComplete="current-password"
              />
            </FormField>

            <div className="login-options">
              <label className="login-checkbox">
                <input
                  type="checkbox"
                  {...register('rememberMe')}
                />
                <span>Remember me</span>
              </label>

              <a href="/auth/password-reset" className="login-forgot-link">
                Forgot password?
              </a>
            </div>

            <Button
              type="submit"
              variant="primary"
              fullWidth
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Signing in...' : 'Sign in'}
            </Button>
          </form>

          <div className="login-footer">
            <p className="login-footer-text">
              Don't have an account?{' '}
              <a href="/auth/register" className="login-register-link">
                Contact your administrator
              </a>
            </p>
          </div>
        </Card>
      </div>
    </div>
  );
};
