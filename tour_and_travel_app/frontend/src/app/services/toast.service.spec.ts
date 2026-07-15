import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';
import { vi } from 'vitest';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should show a toast and auto-remove it', () => {
    service.show('Test message', 'success');
    let toasts = service.toasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].message).toBe('Test message');
    expect(toasts[0].type).toBe('success');
    expect(toasts[0].closing).toBeFalsy();

    // After 2700ms, startClose is called, setting closing to true
    vi.advanceTimersByTime(2700);
    toasts = service.toasts();
    expect(toasts[0].closing).toBe(true);

    // After another 300ms, it is removed
    vi.advanceTimersByTime(300);
    toasts = service.toasts();
    expect(toasts.length).toBe(0);
  });

  it('should manually remove a toast', () => {
    service.show('Manual remove', 'error');
    let toasts = service.toasts();
    expect(toasts.length).toBe(1);
    const toastId = toasts[0].id;

    service.remove(toastId);
    
    toasts = service.toasts();
    // Setting closing to true happens immediately in startClose
    expect(toasts[0].closing).toBe(true);

    // Should remove from array after 300ms
    vi.advanceTimersByTime(300);
    toasts = service.toasts();
    expect(toasts.length).toBe(0);
    
    // Clear remaining timers from show()
    vi.advanceTimersByTime(2700);
  });
  
  it('should not throw when removing non-existent toast', () => {
    expect(() => service.remove(999)).not.toThrow();
  });
});
