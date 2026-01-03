import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, AuthUser } from '../../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = environment.apiUrl;

  private readonly TOKEN_KEY = 'portvault_token';
  private readonly USER_KEY = 'portvault_user';
  private readonly TOKEN_EXPIRY_KEY = 'portvault_token_expiry';

  private currentUserSignal = signal<AuthUser | null>(this.getUserFromStorage());
  private isAuthenticatedSignal = signal<boolean>(this.checkAuthentication());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = this.isAuthenticatedSignal.asReadonly();

  constructor() {
    // Check token expiry on service initialization
    this.validateToken();
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/Auth/register`, request).pipe(
      tap((response) => this.handleAuthResponse(response)),
      catchError((error) => {
        console.error('Registration failed:', error);
        throw error;
      })
    );
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/Auth/login`, request).pipe(
      tap((response) => this.handleAuthResponse(response)),
      catchError((error) => {
        console.error('Login failed:', error);
        throw error;
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.TOKEN_EXPIRY_KEY);
    this.currentUserSignal.set(null);
    this.isAuthenticatedSignal.set(false);
    this.router.navigate(['/auth']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.TOKEN_EXPIRY_KEY, response.expiresUtc);

    const user: AuthUser = {
      id: this.extractUserIdFromToken(response.accessToken),
      username: response.username,
      email: response.email,
    };

    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSignal.set(user);
    this.isAuthenticatedSignal.set(true);
    this.router.navigate(['/']);
  }

  private getUserFromStorage(): AuthUser | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    return userJson ? JSON.parse(userJson) : null;
  }

  private checkAuthentication(): boolean {
    const token = this.getToken();
    if (!token) return false;

    const expiry = localStorage.getItem(this.TOKEN_EXPIRY_KEY);
    if (!expiry) return false;

    return new Date(expiry) > new Date();
  }

  private validateToken(): void {
    if (!this.checkAuthentication()) {
      this.logout();
    }
  }

  private extractUserIdFromToken(token: string): string {
    try {
      const payload = token.split('.')[1];
      const decodedPayload = JSON.parse(atob(payload));
      return (
        decodedPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || ''
      );
    } catch {
      return '';
    }
  }
}
