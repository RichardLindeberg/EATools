# Item-075: Setup Frontend React Project with TypeScript

**Status:** 🟢 Ready  
**Priority:** P1 - HIGH  
**Effort:** 8-12 hours  
**Created:** 2026-01-17  
**Owner:** Frontend Team

---

## Problem Statement

The EATool project currently has a complete backend API and comprehensive UI specifications, but lacks a frontend application. We need to initialize a modern React project with TypeScript that serves as the foundation for all UI development.

Without a frontend project setup, the development team cannot begin implementing the UI specifications, and end users have no way to interact with the backend API services.

This is a critical blocker for all UI development work. The project must follow modern best practices, support the tech stack defined in the UI specifications, and provide a scalable foundation for the 40+ components and 9 entity types that need to be implemented.

---

## Affected Files

**Create:**
- `frontend/package.json` - Project dependencies and scripts
- `frontend/tsconfig.json` - TypeScript configuration
- `frontend/vite.config.ts` - Vite build configuration
- `frontend/src/main.tsx` - Application entry point
- `frontend/src/App.tsx` - Root application component
- `frontend/src/vite-env.d.ts` - Vite type declarations
- `frontend/.eslintrc.json` - ESLint configuration
- `frontend/.prettierrc` - Prettier configuration
- `frontend/src/api/client.ts` - API client configuration
- `frontend/src/types/index.ts` - Global TypeScript types
- `frontend/src/styles/index.css` - Global styles
- `frontend/README.md` - Frontend documentation

---

## Specifications

- [spec/spec-design-ui-architecture.md](../spec/spec-design-ui-architecture.md) - Design system requirements
- [spec/spec-ui-api-integration.md](../spec/spec-ui-api-integration.md) - API client requirements
- [READINESS-SUMMARY.md](../READINESS-SUMMARY.md) - Frontend development readiness
- [BACKEND-UI-ALIGNMENT.md](../BACKEND-UI-ALIGNMENT.md) - Backend-UI alignment

---

## Detailed Tasks

### Project Initialization
- [ ] Create frontend directory in project root
- [ ] Initialize React 18+ project with Vite
- [ ] Configure TypeScript with strict mode
- [ ] Setup ESLint with React and TypeScript rules
- [ ] Setup Prettier for code formatting
- [ ] Add .gitignore for frontend artifacts

### Core Dependencies
- [ ] Install React 18+ and React DOM
- [ ] Install TypeScript 5+
- [ ] Install React Router v6+
- [ ] Install Axios for HTTP requests
- [ ] Install TanStack Query (React Query) for API state management
- [ ] Install React Hook Form for form handling
- [ ] Install Tailwind CSS for styling

### Development Infrastructure
- [ ] Configure Vite dev server (proxy to backend API on port 8000)
- [ ] Setup hot module replacement (HMR)
- [ ] Configure path aliases (@/ for src/)
- [ ] Setup development environment variables (.env.development)
- [ ] Create npm scripts (dev, build, preview, test, lint)

### API Client Setup
- [ ] Create Axios instance with base URL configuration
- [ ] Implement request interceptor for auth tokens
- [ ] Implement response interceptor for error handling
- [ ] Create API client wrapper functions
- [ ] Setup TanStack Query client with defaults

### Project Structure
- [ ] Create src/components/ directory for UI components
- [ ] Create src/pages/ directory for page components
- [ ] Create src/api/ directory for API client code
- [ ] Create src/hooks/ directory for custom React hooks
- [ ] Create src/types/ directory for TypeScript types
- [ ] Create src/utils/ directory for utility functions
- [ ] Create src/styles/ directory for CSS/styling

### Testing Setup
- [ ] Install Vitest for unit testing
- [ ] Install React Testing Library
- [ ] Configure test environment
- [ ] Add test script to package.json
- [ ] Create example test file

### Documentation
- [ ] Create frontend README with setup instructions
- [ ] Document project structure
- [ ] Document available npm scripts
- [ ] Add contribution guidelines
- [ ] Document environment variables

### Verification
- [ ] Verify dev server starts successfully
- [ ] Verify backend API connection (test /health endpoint)
- [ ] Verify HMR works correctly
- [ ] Verify TypeScript compilation with no errors
- [ ] Verify ESLint runs with no errors
- [ ] Verify build produces production artifacts
- [ ] Test production preview mode

---

## Acceptance Criteria

- [ ] React 18+ project successfully created and running
- [ ] TypeScript configured with strict mode enabled
- [ ] Dev server starts on http://localhost:3000 (or similar)
- [ ] API client can successfully call backend health endpoint
- [ ] All dependencies installed and documented
- [ ] Project structure follows best practices
- [ ] Hot module replacement working
- [ ] Build produces optimized production bundle
- [ ] README documentation complete
- [ ] All team members can run project locally

---

## Dependencies

**Blocked by:** None  
**Blocks:** All UI implementation items (Items 076-085)

---

## Notes

- Use Vite instead of Create React App for faster build times
- Configure Vite proxy to backend API (http://localhost:8000)
- Use TypeScript strict mode from the start
- Follow the tech stack from READINESS-SUMMARY.md
- Ensure Node.js 18+ is used
- Consider using pnpm or yarn instead of npm for faster installs
