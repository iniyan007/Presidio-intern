import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, HttpClientModule, RouterModule],
  templateUrl: './auth.html',
  styleUrl: './auth.css'
})
export class AuthComponent implements OnInit {
  isLoginTab = signal<boolean>(true);
  loginForm: FormGroup;
  signupForm: FormGroup;
  
  errorMessage = signal<string>('');
  successMessage = signal<string>('');
  isLoading = signal<boolean>(false);
  
  showLoginPassword = signal<boolean>(false);
  showSignupPassword = signal<boolean>(false);

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toastService = inject(ToastService);

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['tab'] === 'signup') {
        this.isLoginTab.set(false);
      } else {
        this.isLoginTab.set(true);
      }
    });
  }

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

    this.loginForm.get('email')?.valueChanges.subscribe(val => {
      if (val && val !== val.toLowerCase()) {
        this.loginForm.get('email')?.patchValue(val.toLowerCase(), { emitEvent: false });
      }
    });

    this.signupForm.get('email')?.valueChanges.subscribe(val => {
      if (val && val !== val.toLowerCase()) {
        this.signupForm.get('email')?.patchValue(val.toLowerCase(), { emitEvent: false });
      }
    });
  }

  onLogin() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
    
    this.authService.login(this.loginForm.value).subscribe({
      next: (res: any) => {
        this.isLoading.set(false);
        this.toastService.show('Logged in successfully', 'success');
        if (!this.authService.isEmailVerified()) {
          this.authService.sendOtp().subscribe();
          this.router.navigate(['/verify-email']);
        } else {
          if (this.authService.getUserRole() === 'Admin') {
            this.router.navigate(['/admin/dashboard']);
          } else {
            this.router.navigate(['/']);
          }
        }
      },
      error: (err: any) => {
        this.isLoading.set(false);
        if (err.status === 401) {
          this.toastService.show('Invalid email or password. Please try again.', 'error');
        } else {
          this.toastService.show(err.error?.message || 'Login failed. Please try again later.', 'error');
        }
      }
    });
  }

  onSignup() {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.authService.register(this.signupForm.value).subscribe({
      next: (res: any) => {
        this.isLoading.set(false);
        this.toastService.show('Registration successful! Please login.', 'success');
        this.signupForm.reset();
        this.isLoginTab.set(true); // switch to login
      },
      error: (err: any) => {
        this.isLoading.set(false);
        this.toastService.show(err.error?.message || 'Registration failed', 'error');
      }
    });
  }
}
