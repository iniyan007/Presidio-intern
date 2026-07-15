import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, OnInit, OnDestroy, ViewChildren, QueryList, ElementRef, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './verify-email.html',
  styles: [`
    .glass-panel {
        background: rgba(255, 255, 255, 0.7);
        backdrop-filter: blur(12px);
        -webkit-backdrop-filter: blur(12px);
        border: 1px solid rgba(255, 255, 255, 0.3);
    }
    
    .otp-input:focus {
        box-shadow: 0 0 0 2px rgba(0, 32, 69, 0.1);
        transform: translateY(-2px);
    }

    .animated-gradient {
        background: linear-gradient(135deg, #f7f9fb 0%, #d6e3ff 50%, #f7f9fb 100%);
        background-size: 200% 200%;
        animation: flow 15s ease infinite;
    }

    @keyframes flow {
        0% { background-position: 0% 50%; }
        50% { background-position: 100% 50%; }
        100% { background-position: 0% 50%; }
    }
  `]
})
export class VerifyEmailComponent implements OnInit, OnDestroy {
  private destroyRef = inject(DestroyRef);
  otpForm: FormGroup;
  timeLeft = signal<number>(50);
  timerInterval: any;
  isLoading = signal<boolean>(false);
  isResending = signal<boolean>(false);
  isSuccess = signal<boolean>(false);
  maskedEmail = signal<string>('');

  @ViewChildren('otpInput') otpInputs!: QueryList<ElementRef>;

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  constructor() {
    this.otpForm = this.fb.group({
      otp: this.fb.array([
        ['', [Validators.required, Validators.pattern('[0-9]')]],
        ['', [Validators.required, Validators.pattern('[0-9]')]],
        ['', [Validators.required, Validators.pattern('[0-9]')]],
        ['', [Validators.required, Validators.pattern('[0-9]')]],
        ['', [Validators.required, Validators.pattern('[0-9]')]],
        ['', [Validators.required, Validators.pattern('[0-9]')]]
      ])
    });
  }

  ngOnInit() {
    this.startTimer();
    
    // Mask email
    const profile = this.userService.userProfile();
    if (profile?.email) {
      this.maskedEmail.set(this.maskEmailString(profile.email));
    } else {
      this.userService.loadProfile().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(res => {
         if (res?.email) this.maskedEmail.set(this.maskEmailString(res.email));
      });
    }
  }

  ngOnDestroy() {
    this.clearTimer();
  }

  maskEmailString(email: string): string {
    const [name, domain] = email.split('@');
    if (!name || !domain) return email;
    if (name.length <= 2) return `${name}***@${domain}`;
    return `${name.substring(0, 3)}***@${domain}`;
  }

  get otpControls() {
    return (this.otpForm.get('otp') as FormArray).controls;
  }

  onInput(event: any, index: number) {
    const value = event.target.value;
    if (value && /^[0-9]$/.test(value)) {
      if (index < 5) {
        this.otpInputs.toArray()[index + 1].nativeElement.focus();
      }
    } else {
      (this.otpForm.get('otp') as FormArray).at(index).setValue('');
    }
  }

  onKeyDown(event: KeyboardEvent, index: number) {
    if (event.key === 'Backspace') {
      const currentValue = (this.otpForm.get('otp') as FormArray).at(index).value;
      if (!currentValue && index > 0) {
        this.otpInputs.toArray()[index - 1].nativeElement.focus();
      }
    }
  }

  startTimer() {
    this.timeLeft.set(50);
    this.clearTimer();
    this.timerInterval = setInterval(() => {
      const current = this.timeLeft();
      if (current > 0) {
        this.timeLeft.set(current - 1);
      } else {
        this.clearTimer();
      }
    }, 1000);
  }

  clearTimer() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  formatTime(): string {
    const seconds = this.timeLeft();
    return `00:${seconds.toString().padStart(2, '0')}`;
  }

  resendOtp() {
    if (this.timeLeft() > 0) return;
    
    this.isResending.set(true);
    this.authService.sendOtp().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isResending.set(false);
        this.toastService.show('OTP sent successfully', 'success');
        this.startTimer();
      },
      error: (err) => {
        this.isResending.set(false);
        this.toastService.show('Failed to send OTP', 'error');
      }
    });
  }

  verifyOtp() {
    if (this.otpForm.invalid) return;

    const otpArray = this.otpForm.value.otp;
    const otpString = otpArray.join('');
    
    this.isLoading.set(true);
    this.authService.verifyOtp(otpString).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.isSuccess.set(true);
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 2500);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.toastService.show(err.error?.message || 'Verification failed. Invalid OTP.', 'error');
        (this.otpForm.get('otp') as FormArray).reset();
        this.otpInputs.toArray()[0].nativeElement.focus();
      }
    });
  }
}
