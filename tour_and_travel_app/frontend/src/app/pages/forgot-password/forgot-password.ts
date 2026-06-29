import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './forgot-password.html',
})
export class ForgotPasswordComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  step = signal<number>(1);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');
  
  email = signal<string>('');
  resetToken = signal<string>('');

  emailForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  otpForm: FormGroup = this.fb.group({
    otp: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
  });

  passwordForm: FormGroup = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  });

  ngOnInit() {
    this.emailForm.get('email')?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(val => {
      if (val && val !== val.toLowerCase()) {
        this.emailForm.get('email')?.patchValue(val.toLowerCase(), { emitEvent: false });
      }
    });
  }

  requestOtp() {
    if (this.emailForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');
    const userEmail = this.emailForm.value.email;

    this.authService.forgotPassword(userEmail).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        this.email.set(userEmail);
        this.toastService.show('OTP sent to your email', 'success');
        this.step.set(2);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to send OTP.');
      }
    });
  }

  verifyOtp() {
    if (this.otpForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.verifyResetOtp(this.email(), this.otpForm.value.otp).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        this.resetToken.set(res.resetToken);
        this.step.set(3);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Invalid or expired OTP.');
      }
    });
  }

  resetPassword() {
    if (this.passwordForm.invalid) return;

    const { newPassword, confirmPassword } = this.passwordForm.value;

    if (newPassword !== confirmPassword) {
      this.errorMessage.set('Passwords do not match.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    const payload = {
      email: this.email(),
      resetToken: this.resetToken(),
      newPassword: newPassword
    };

    this.authService.resetPassword(payload).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.toastService.show('Password reset successfully!', 'success');
        this.router.navigate(['/auth'], { queryParams: { tab: 'login' } });
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to reset password.');
      }
    });
  }
}
