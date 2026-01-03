import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AuthComponent } from './auth';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';

describe('AuthComponent', () => {
  let component: AuthComponent;
  let fixture: ComponentFixture<AuthComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthComponent],
      providers: [provideHttpClient(), provideRouter([]), provideAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have login form with email and password fields', () => {
    expect(component.loginForm.get('email')).toBeTruthy();
    expect(component.loginForm.get('password')).toBeTruthy();
  });

  it('should have register form with username, email and password fields', () => {
    expect(component.registerForm.get('username')).toBeTruthy();
    expect(component.registerForm.get('email')).toBeTruthy();
    expect(component.registerForm.get('password')).toBeTruthy();
  });

  it('should validate email format in login form', () => {
    const emailControl = component.loginForm.get('email');
    emailControl?.setValue('invalid-email');
    expect(emailControl?.hasError('email')).toBeTruthy();

    emailControl?.setValue('valid@email.com');
    expect(emailControl?.hasError('email')).toBeFalsy();
  });

  it('should validate password minimum length', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('12345');
    expect(passwordControl?.hasError('minlength')).toBeTruthy();

    passwordControl?.setValue('123456');
    expect(passwordControl?.hasError('minlength')).toBeFalsy();
  });
});
