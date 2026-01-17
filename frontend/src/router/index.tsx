/**
 * Router Setup
 * Initializes React Router v6 with the route configuration
 */

import { createBrowserRouter } from 'react-router-dom';
import { routes } from './routes';

export const router = createBrowserRouter(routes, {
  basename: '/',
});
