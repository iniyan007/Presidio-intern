import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  loginForm: FormGroup;
  error: string = '';
  loading: boolean = false;
  pendingApproval: boolean = false; // ← new: shows the "waiting" modal

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.loading = true;
    this.error = '';
    this.pendingApproval = false;

    this.authService.login(this.loginForm.value).subscribe({
      next: (res) => {
        const role = res.user.role;
        if (role === 'Admin') this.router.navigate(['/admin/dashboard']);
        else if (role === 'Operator') this.router.navigate(['/operator/dashboard']);
        else this.router.navigate(['/user/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 403) {
          // Operator pending admin approval – show friendly modal
          this.pendingApproval = true;
        } else {
          this.error = err.error?.message || 'Invalid email or password';
        }
      }
    });
  }
}
