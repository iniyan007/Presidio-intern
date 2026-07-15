import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastComponent } from './toast';
import { ToastService } from '../../services/toast.service';
import { signal } from '@angular/core';

describe('ToastComponent', () => {
  let component: ToastComponent;
  let fixture: ComponentFixture<ToastComponent>;
  let toastServiceSpy: any;

  beforeEach(async () => {
    toastServiceSpy = {
      toasts: signal([])
    };

    await TestBed.configureTestingModule({
      imports: [ToastComponent],
      providers: [
        { provide: ToastService, useValue: toastServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ToastComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
