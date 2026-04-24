import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  fb = inject(FormBuilder);
  authService = inject(AuthService);
  router = inject(Router);

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  error: string = '';
  loading: boolean = false;

  onSubmit() {
    if (this.loginForm.valid) {
      this.loading = true;
      this.error = '';
      this.authService.login(this.loginForm.value).subscribe({
        next: (res) => {
          this.loading = false;
          // Route based on role
          if (res.user.role === 'ADMIN') {
            this.router.navigate(['/admin']);
          } else if (res.user.role === 'OPERATOR') {
            this.router.navigate(['/operator']);
          } else {
            this.router.navigate(['/']);
          }
        },
        error: (err) => {
          this.loading = false;
          this.error = err.error || 'Invalid credentials';
        }
      });
    } else {
      this.loginForm.markAllAsTouched();
    }
  }
}
