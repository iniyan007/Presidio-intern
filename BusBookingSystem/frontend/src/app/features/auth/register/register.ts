import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  fb = inject(FormBuilder);
  authService = inject(AuthService);
  router = inject(Router);

  registerForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    phone: ['', Validators.required],
    age: ['', [Validators.required, Validators.min(18)]],
    gender: ['', Validators.required],
    roleId: [1] // Default to USER (roleId = 1). Operator will be registered differently or we give an option? The prompt says "Operator role: the operator should have the registration which is approved by admin". But wait, Operator Controller has a "Register" endpoint: `[HttpPost("register")] public async Task<IActionResult> Register(CreateOperatorRequest request)` which uses `[Authorize]`, meaning user registers as normal, then registers as Operator! So here roleId is always 1 (USER).
  });

  error: string = '';
  loading: boolean = false;

  onSubmit() {
    if (this.registerForm.valid) {
      this.loading = true;
      this.error = '';
      this.authService.register(this.registerForm.value).subscribe({
        next: (res) => {
          this.loading = false;
          alert('Registration successful! Please login.');
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.loading = false;
          this.error = err.error?.message || err.error || 'Registration failed';
        }
      });
    } else {
      this.registerForm.markAllAsTouched();
    }
  }
}
