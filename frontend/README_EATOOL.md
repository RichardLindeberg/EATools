# EATool Frontend

Modern React 18 + TypeScript + Vite frontend for the EATool application.

## Quick Start

### Prerequisites
- Node.js 18+ (or use nvm)
- npm 9+ or yarn 3+

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Open browser to http://localhost:3000
```

The development server will proxy API requests to `http://localhost:8000`.

## Available Scripts

### Development
```bash
npm run dev          # Start dev server with HMR
npm run preview      # Preview production build locally
```

### Building
```bash
npm run build        # Build for production
npm run type-check   # Type check without building
```

### Code Quality
```bash
npm run lint         # Run ESLint
npm run format       # Format code with Prettier
```

## Project Structure

```
frontend/
├── src/
│   ├── api/              # API client and hooks
│   │   ├── client.ts     # Axios instance and interceptors
│   │   └── queryClient.ts # React Query configuration
│   ├── components/       # Reusable React components
│   ├── pages/            # Page components (routes)
│   ├── hooks/            # Custom React hooks
│   ├── types/            # TypeScript type definitions
│   ├── utils/            # Utility functions
│   ├── styles/           # Global styles
│   ├── App.tsx           # Root component
│   └── main.tsx          # Application entry point
├── public/               # Static assets
├── vite.config.ts        # Vite configuration
├── tsconfig.json         # TypeScript configuration
├── .eslintrc.json        # ESLint configuration
└── .prettierrc            # Prettier configuration
```

## Configuration

### TypeScript
- Strict mode enabled
- Path aliases: `@/` → `src/`
- Target: ES2020+

### Vite
- Dev server: http://localhost:3000
- API proxy: `/api` → `http://localhost:8000`
- Build target: `dist/`

### Environment Variables

**Development** (`.env.development`):
```
VITE_API_BASE_URL=http://localhost:8000/api
VITE_ENABLE_DEBUG=true
```

**Production** (`.env.production`):
```
VITE_API_BASE_URL=/api
VITE_ENABLE_DEBUG=false
```

## Key Dependencies

### Core
- **React 18**: UI library
- **React Router v6**: Routing
- **TypeScript 5**: Type safety

### API & State
- **Axios**: HTTP client
- **TanStack Query v5**: Server state management
- **React Hook Form**: Form handling

### Development
- **Vite 5**: Build tool and dev server
- **Vitest**: Unit testing
- **ESLint**: Linting
- **Prettier**: Code formatting

## API Integration

The frontend connects to the backend API via Axios with:
- Automatic auth token injection (from localStorage)
- Request/response interceptors
- Error handling (401 redirects to login)
- Proxy configuration for development

### API Client Usage

```typescript
import { apiClient, checkHealth } from '@/api/client'

// Check backend health
const health = await checkHealth()

// Make custom requests
const response = await apiClient.get('/entities')
const created = await apiClient.post('/entities', { name: 'New' })
```

### React Query Hooks

```typescript
import { useEntityList, useEntity } from '@/hooks/useEntity'

// List entities
const { data, isLoading } = useEntityList('servers')

// Get single entity
const { data: entity } = useEntity('servers', id)

// Create entity
const { mutate: create } = useCreateEntity('servers')
```

## Development Workflow

### Creating a New Page

1. Create component in `src/pages/NewPage.tsx`
2. Add route in `App.tsx`
3. Link in navigation

### Creating a New Component

1. Create component in `src/components/MyComponent.tsx`
2. Export from components index
3. Import and use in pages

### Adding API Integration

1. Add types in `src/types/index.ts`
2. Use hooks from `src/hooks/useEntity.ts`
3. Handle loading/error states

## Testing

```bash
npm run test              # Run tests
npm run test:watch       # Watch mode
npm run test:coverage    # Coverage report
```

Tests use Vitest + React Testing Library.

## Type Safety

The project enforces strict TypeScript:
- All functions typed
- Props use interfaces
- No `any` types (enforced by linter)

## Performance

Optimizations:
- Code splitting via Vite
- Image optimization
- CSS minification
- Production build: ~200KB gzipped

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari 14+
- Edge (latest)

## Troubleshooting

### Dev server not starting
```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### API connection issues
- Ensure backend is running on http://localhost:8000
- Check browser console for CORS errors
- Verify proxy config in vite.config.ts

### TypeScript errors
```bash
npm run type-check  # Full type check
```

## Contributing

1. Follow the project structure
2. Use TypeScript for all new code
3. Run linter before committing: `npm run lint`
4. Add tests for new features
5. Update documentation

## Next Steps

See backlog items for upcoming features:
- Item-076: Core Component Library
- Item-077: Authentication Pages
- Item-078: Routing & Navigation
- Item-079-089: Entity management features

---

**Status:** 🟡 In Progress (Item-075)  
**Last Updated:** 2026-01-17
