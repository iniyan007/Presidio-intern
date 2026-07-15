import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NavbarComponent } from './navbar';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { WishlistService } from '../../services/wishlist.service';
import { NotificationService } from '../../services/notification.service';
import { ChatService } from '../../services/chat.service';
import { AgencyService } from '../../services/agency.service';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { vi } from 'vitest';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  
  let authServiceSpy: any;
  let userServiceSpy: any;
  let toastServiceSpy: any;
  let wishlistServiceSpy: any;
  let notificationServiceSpy: any;
  let chatServiceSpy: any;
  let agencyServiceSpy: any;
  let routerSpy: any;

  beforeEach(async () => {
    authServiceSpy = {
      isAuthenticated: vi.fn().mockReturnValue(false),
      getUserRole: vi.fn().mockReturnValue(null),
      logout: vi.fn()
    };
    
    userServiceSpy = {
      userProfile: signal(null)
    };
    
    toastServiceSpy = {
      show: vi.fn()
    };
    
    wishlistServiceSpy = {
      wishlistCount: signal(0),
      clearWishlists: vi.fn()
    };
    
    notificationServiceSpy = {
      loadNotifications: vi.fn(),
      startConnection: vi.fn(),
      stopConnection: vi.fn(),
      notifications: signal([]),
      unreadCount: signal(0),
      markAsRead: vi.fn(),
      markAllAsRead: vi.fn()
    };
    
    chatServiceSpy = {
      startConnection: vi.fn(),
      stopConnection: vi.fn(),
      getThreads: vi.fn().mockReturnValue(of([])),
      threads: signal([])
    };
    
    agencyServiceSpy = {
      getMyPackagerStatus: vi.fn().mockReturnValue(of({ deactivatedAt: null }))
    };

    // Note: since the constructor has an effect that reads authService.isAuthenticated(),
    // we want to mock localStorage as well just in case.
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation((key) => {
      if (key === 'jwt_token') return 'fake-jwt-token';
      return null;
    });

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy },
        { provide: WishlistService, useValue: wishlistServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy },
        { provide: ChatService, useValue: chatServiceSpy },
        { provide: AgencyService, useValue: agencyServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    
    const router = TestBed.inject(Router);
    routerSpy = {
      navigate: vi.spyOn(router, 'navigate').mockResolvedValue(true)
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create and initialize for unauthenticated user', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(false);
    fixture.detectChanges();
    
    expect(component).toBeTruthy();
    expect(notificationServiceSpy.stopConnection).toHaveBeenCalled();
    expect(chatServiceSpy.stopConnection).toHaveBeenCalled();
  });

  it('should create and initialize for authenticated guest user', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);
    authServiceSpy.getUserRole.mockReturnValue('Guest');
    
    fixture.detectChanges();

    expect(notificationServiceSpy.loadNotifications).toHaveBeenCalled();
    expect(notificationServiceSpy.startConnection).toHaveBeenCalled();
    expect(chatServiceSpy.startConnection).toHaveBeenCalledWith('fake-jwt-token');
    expect(chatServiceSpy.getThreads).toHaveBeenCalled();
    expect(agencyServiceSpy.getMyPackagerStatus).not.toHaveBeenCalled();
  });

  it('should create and initialize for authenticated packager user', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);
    authServiceSpy.getUserRole.mockReturnValue('Packager');
    agencyServiceSpy.getMyPackagerStatus.mockReturnValue(of({ deactivatedAt: '2025-01-01' }));
    
    fixture.detectChanges();

    expect(agencyServiceSpy.getMyPackagerStatus).toHaveBeenCalled();
    expect(component.isPackagerDeactivated()).toBeTruthy();
  });

  it('should toggle notification menu and close mobile menu', () => {
    component.isMobileMenuOpen.set(true);
    component.toggleNotification();
    
    expect(component.isNotificationOpen()).toBeTruthy();
    expect(component.isMobileMenuOpen()).toBeFalsy();
  });

  it('should toggle mobile menu and close notification menu', () => {
    component.isNotificationOpen.set(true);
    component.toggleMobileMenu();
    
    expect(component.isMobileMenuOpen()).toBeTruthy();
    expect(component.isNotificationOpen()).toBeFalsy();
  });

  it('should handle logout', () => {
    component.logout();
    
    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(wishlistServiceSpy.clearWishlists).toHaveBeenCalled();
    expect(toastServiceSpy.show).toHaveBeenCalledWith('Logged out successfully', 'info');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should mark all notifications as read', () => {
    component.markAllAsRead();
    
    expect(notificationServiceSpy.markAllAsRead).toHaveBeenCalled();
  });

  it('should handle notification click via markAsRead', () => {
    component.isNotificationOpen.set(true);
    component.markAsRead('notif-id');
    
    expect(notificationServiceSpy.markAsRead).toHaveBeenCalledWith('notif-id');
    expect(component.isNotificationOpen()).toBeFalsy();
  });

  it('should calculate unreadChatCount correctly', () => {
    chatServiceSpy.threads.set([
      { unreadCount: 2 },
      { unreadCount: 3 },
      { unreadCount: 0 }
    ]);
    
    expect(component.unreadChatCount).toBe(5);
  });
});
