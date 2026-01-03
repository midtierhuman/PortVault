# PortVault Authentication System

## Overview

A complete authentication system for PortVault using Angular Material Design, built as a zoneless standalone Angular application.

## Features

### 🔐 Authentication Landing Page

- **Modern Material Design**: Beautiful gradient background with glassmorphism effects
- **Dual Forms**: Login and Register tabs in a single interface
- **Real-time Validation**: Instant feedback on form inputs
- **Password Visibility Toggle**: User-friendly password field with show/hide functionality
- **Loading States**: Visual feedback during API calls
- **Responsive Design**: Works seamlessly on mobile and desktop

### 🛡️ Security Features

- JWT token management with automatic expiration handling
- HTTP interceptor for automatic token injection
- Secure token storage in localStorage
- Route guards for protected routes
- Automatic redirect for unauthorized access

### 🎨 User Experience

- Clean Material Design interface
- Snackbar notifications for success/error messages
- Feature highlights on landing page
- User menu with profile info in header
- Smooth animations and transitions

## File Structure

```
src/app/
├── core/
│   ├── guards/
│   │   └── auth.guard.ts          # Route guards (authGuard, publicGuard)
│   ├── interceptors/
│   │   └── auth.interceptor.ts    # HTTP interceptor for auth tokens
│   └── services/
│       └── auth.service.ts        # Authentication service with signals
├── features/
│   └── auth/
│       ├── auth.ts                # Auth component logic
│       ├── auth.html              # Auth component template
│       ├── auth.scss              # Auth component styles
│       └── auth.spec.ts           # Auth component tests
├── models/
│   └── auth.model.ts              # Auth-related TypeScript interfaces
└── shared/
    └── ui/
        └── header/                # Updated with user menu
```

## API Integration

### Registration Endpoint

**URL**: `POST https://localhost:7061/api/Auth/register`

**Request**:

```json
{
  "username": "string",
  "email": "user@example.com",
  "password": "string"
}
```

**Response**:

```json
{
  "accessToken": "eyJhbGc...",
  "expiresUtc": "2026-01-03T18:30:36.0714715Z",
  "username": "string",
  "email": "user@example.com"
}
```

### Login Endpoint

**URL**: `POST https://localhost:7061/api/Auth/login`

**Request**:

```json
{
  "email": "user@example.com",
  "password": "string"
}
```

**Response**: Same as registration

## Usage

### Service Methods

```typescript
// Inject the service
private authService = inject(AuthService);

// Register a new user
this.authService.register({
  username: 'john',
  email: 'john@example.com',
  password: 'password123'
}).subscribe();

// Login
this.authService.login({
  email: 'john@example.com',
  password: 'password123'
}).subscribe();

// Logout
this.authService.logout();

// Check authentication status (signal)
const isAuth = this.authService.isAuthenticated();

// Get current user (signal)
const user = this.authService.currentUser();
```

### Route Protection

```typescript
// Protect routes requiring authentication
{
  path: 'dashboard',
  component: DashboardComponent,
  canActivate: [authGuard]
}

// Public routes (redirect if already authenticated)
{
  path: 'auth',
  component: AuthComponent,
  canActivate: [publicGuard]
}
```

## Configuration

The API URL is configured in `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7061/api',
};
```

## Validation Rules

### Login Form

- **Email**: Required, must be valid email format
- **Password**: Required, minimum 6 characters

### Register Form

- **Username**: Required, minimum 3 characters
- **Email**: Required, must be valid email format
- **Password**: Required, minimum 6 characters

## State Management

The auth service uses Angular signals for reactive state:

- `currentUser`: Signal containing authenticated user info
- `isAuthenticated`: Signal indicating authentication status

## Token Management

- Tokens are stored in localStorage with keys:

  - `portvault_token`: JWT access token
  - `portvault_user`: User information
  - `portvault_token_expiry`: Token expiration timestamp

- Automatic validation on service initialization
- Automatic logout when token expires
- Token automatically added to HTTP requests via interceptor

## Styling

The auth page features:

- Purple gradient background (from #667eea to #764ba2)
- Glassmorphism effects for feature cards
- Material Design elevation and shadows
- Responsive breakpoints at 600px
- Smooth transitions and animations

## Testing

Run tests with:

```bash
npm test
```

The auth component includes tests for:

- Component creation
- Form validation
- Email format validation
- Password length validation
