import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, HttpClientModule],
  templateUrl: './auth.html',
  styleUrl: './auth.css'
})
export class AuthComponent {
  isLoginTab = signal<boolean>(true);
  loginForm: FormGroup;
  signupForm: FormGroup;
  
  errorMessage = signal<string>('');
  successMessage = signal<string>('');
  isLoading = signal<boolean>(false);

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });

    this.signupForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.pattern('^[6-9]\\d{9}$')]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  onLogin() {
    if (this.loginForm.invalid) return;
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
    
    this.authService.login(this.loginForm.value).subscribe({
      next: (res: any) => {
        this.isLoading.set(false);
        this.successMessage.set('Login successful! JWT stored.');
        console.log('Login successful:', res);
        // We will navigate to dashboard later
      },
      error: (err: any) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Login failed');
      }
    });
  }

  onSignup() {
    if (this.signupForm.invalid) return;
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.authService.register(this.signupForm.value).subscribe({
      next: (res: any) => {
        this.isLoading.set(false);
        this.successMessage.set('Registration successful! Please login.');
        this.signupForm.reset();
        this.isLoginTab.set(true); // switch to login
      },
      error: (err: any) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Registration failed');
      }
    });
  }
}
