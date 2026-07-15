import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { NotificationService } from './notification.service';
import { AuthService } from './auth.service';
import { ToastService } from './toast.service';
import { environment } from '../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { vi } from 'vitest';

const mocks = vi.hoisted(() => {
  const mockHubConnection = {
    start: vi.fn().mockResolvedValue(undefined),
    stop: vi.fn().mockResolvedValue(undefined),
    on: vi.fn(),
    state: 'Connected'
  };

  const mockBuilder = {
    withUrl: vi.fn().mockReturnThis(),
    withAutomaticReconnect: vi.fn().mockReturnThis(),
    build: vi.fn().mockReturnValue(mockHubConnection)
  };

  return { mockHubConnection, mockBuilder };
});

describe('NotificationService', () => {
  let service: NotificationService;
  let httpMock: HttpTestingController;
  let authServiceSpy: any;
  let toastServiceSpy: any;

  beforeEach(() => {
    vi.spyOn(signalR, 'HubConnectionBuilder').mockImplementation(function() {
      return mocks.mockBuilder as any;
    });
    authServiceSpy = {
      isAuthenticated: vi.fn().mockReturnValue(true),
      getToken: vi.fn().mockReturnValue('fake-token')
    };
    toastServiceSpy = { show: vi.fn() };

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        NotificationService,
        { provide: AuthService, useValue: authServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy }
      ]
    });
    
    // clear mocks before inject because constructor calls loadNotifications and startConnection
    vi.clearAllMocks();
    
    service = TestBed.inject(NotificationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    vi.clearAllMocks();
  });

  it('should be created and start connection if authenticated', () => {
    expect(service).toBeTruthy();
    
    // Constructor triggers loadNotifications
    const req = httpMock.expectOne(`${environment.apiUrl}/Notifications`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: [] });

    expect(mocks.mockHubConnection.start).toHaveBeenCalled();
  });

  it('should load notifications', () => {
    // Flush initial call from constructor
    httpMock.expectOne(`${environment.apiUrl}/Notifications`).flush({ success: true, data: [] });

    const mockResponse = {
      success: true,
      data: [{ id: '1', title: 'Test', isRead: false }]
    };

    service.loadNotifications();

    const req = httpMock.expectOne(`${environment.apiUrl}/Notifications`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);

    expect(service.notifications().length).toBe(1);
    expect(service.unreadCount()).toBe(1);
  });

  it('should stop connection and clear notifications', () => {
    // Flush initial call
    httpMock.expectOne(`${environment.apiUrl}/Notifications`).flush({ success: true, data: [] });

    service.stopConnection();
    expect(mocks.mockHubConnection.stop).toHaveBeenCalled();
    expect(service.notifications().length).toBe(0);
  });

  it('should mark notification as read', () => {
    // Flush initial call
    httpMock.expectOne(`${environment.apiUrl}/Notifications`).flush({ success: true, data: [] });

    // Set initial state
    service.notifications.set([{ id: '1', title: 'Test', isRead: false } as any]);

    service.markAsRead('1');

    const req = httpMock.expectOne(`${environment.apiUrl}/Notifications/1/read`);
    expect(req.request.method).toBe('PUT');
    req.flush({});

    expect(service.notifications()[0].isRead).toBe(true);
    expect(service.unreadCount()).toBe(0);
  });

  it('should mark all notifications as read', () => {
    // Flush initial call
    httpMock.expectOne(`${environment.apiUrl}/Notifications`).flush({ success: true, data: [] });

    // Set initial state
    service.notifications.set([
      { id: '1', title: 'Test 1', isRead: false } as any,
      { id: '2', title: 'Test 2', isRead: false } as any
    ]);

    service.markAllAsRead();

    const req = httpMock.expectOne(`${environment.apiUrl}/Notifications/read-all`);
    expect(req.request.method).toBe('PUT');
    req.flush({});

    expect(service.notifications().every(n => n.isRead)).toBe(true);
    expect(service.unreadCount()).toBe(0);
  });
});
