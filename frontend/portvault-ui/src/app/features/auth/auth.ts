import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../core/services/auth.service';
import { LoginRequest, RegisterRequest } from '../../models/auth.model';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './auth.html',
  styleUrls: ['./auth.scss'],
})
export class AuthComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  loginForm: FormGroup;
  registerForm: FormGroup;
  isLoading = signal(false);
  hideLoginPassword = signal(true);
  hideRegisterPassword = signal(true);

  constructor() {
    this.loginForm = this.fb.group({
      emailOrUsername: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });

    this.registerForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const emailOrUsername = this.loginForm.value.emailOrUsername;
    const isEmail = emailOrUsername.includes('@');

    const loginRequest: LoginRequest = {
      [isEmail ? 'email' : 'username']: emailOrUsername,
      password: this.loginForm.value.password,
    };

    this.authService.login(loginRequest).subscribe({
      next: () => {
        this.snackBar.open('Login successful!', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        const message = error.error?.message || 'Login failed. Please try again.';
        this.snackBar.open(message, 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar'],
        });
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }

  onRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const registerRequest: RegisterRequest = this.registerForm.value;

    this.authService.register(registerRequest).subscribe({
      next: () => {
        this.snackBar.open('Registration successful! Welcome to PortVault!', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        const message = error.error?.message || 'Registration failed. Please try again.';
        this.snackBar.open(message, 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar'],
        });
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }

  getLoginErrorMessage(field: string): string {
    const control = this.loginForm.get(field);
    if (!control?.touched) return '';

    if (control.hasError('required')) {
      if (field === 'emailOrUsername') {
        return 'Email or username is required';
      }
      return `${field.charAt(0).toUpperCase() + field.slice(1)} is required`;
    }
    if (control.hasError('minlength')) {
      return `${field.charAt(0).toUpperCase() + field.slice(1)} must be at least ${
        control.errors?.['minlength'].requiredLength
      } characters`;
    }
    return '';
  }

  getRegisterErrorMessage(field: string): string {
    const control = this.registerForm.get(field);
    if (!control?.touched) return '';

    if (control.hasError('required')) {
      return `${field.charAt(0).toUpperCase() + field.slice(1)} is required`;
    }
    if (control.hasError('email')) {
      return 'Please enter a valid email';
    }
    if (control.hasError('minlength')) {
      return `${field.charAt(0).toUpperCase() + field.slice(1)} must be at least ${
        control.errors?.['minlength'].requiredLength
      } characters`;
    }
    return '';
  }
}
