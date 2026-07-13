import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ChatService } from './chat.service';
import { environment } from '../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { vi } from 'vitest';

const mocks = vi.hoisted(() => {
  const mockHubConnection = {
    start: vi.fn().mockResolvedValue(undefined),
    stop: vi.fn().mockResolvedValue(undefined),
    on: vi.fn(),
    invoke: vi.fn().mockResolvedValue(undefined),
    state: 'Connected'
  };

  const mockBuilder = {
    withUrl: vi.fn().mockReturnThis(),
    withAutomaticReconnect: vi.fn().mockReturnThis(),
    build: vi.fn().mockReturnValue(mockHubConnection)
  };

  return { mockHubConnection, mockBuilder };
});

describe('ChatService', () => {
  let service: ChatService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    vi.spyOn(signalR, 'HubConnectionBuilder').mockImplementation(function() {
      return mocks.mockBuilder as any;
    });
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ChatService]
    });
    service = TestBed.inject(ChatService);
    httpMock = TestBed.inject(HttpTestingController);
    
    // Reset mocks
    vi.clearAllMocks();
    mocks.mockHubConnection.state = 'Connected';
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize thread', () => {
    const mockRequest = { packageId: '123', packagerId: '456' };
    const mockResponse: any = { id: 'thread1' };

    service.getOrInitializeThread(mockRequest).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Messages/threads/init`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockRequest);
    req.flush(mockResponse);
  });

  it('should get threads', () => {
    const mockResponse: any[] = [{ id: 'thread1' }];

    service.getThreads().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Messages/threads`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get thread messages', () => {
    const threadId = 'thread1';
    const mockResponse: any[] = [{ id: 'msg1', text: 'hello' }];

    service.getThreadMessages(threadId).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Messages/threads/${threadId}/messages`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should send a message', () => {
    const mockRequest: any = { threadId: 'thread1', text: 'hello' };
    const mockResponse: any = { id: 'msg1', text: 'hello' };

    service.sendMessage(mockRequest).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Messages/send`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockRequest);
    req.flush(mockResponse);
  });

  it('should mark thread as read', () => {
    const threadId = 'thread1';

    service.markAsRead(threadId).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/Messages/threads/${threadId}/read`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  describe('SignalR', () => {
    it('should start connection if not connected', () => {
      service.startConnection('token');
      expect(mocks.mockHubConnection.start).toHaveBeenCalled();
      expect(mocks.mockHubConnection.on).toHaveBeenCalledWith('ReceiveMessage', expect.any(Function));
    });

    it('should not start connection if already connected', () => {
      service.startConnection('token');
      const startCount = mocks.mockHubConnection.start.mock.calls.length;
      service.startConnection('token');
      expect(mocks.mockHubConnection.start).toHaveBeenCalledTimes(startCount);
    });

    it('should stop connection', () => {
      service.startConnection('token');
      service.stopConnection();
      expect(mocks.mockHubConnection.stop).toHaveBeenCalled();
    });

    it('should join thread if connected', () => {
      service.startConnection('token');
      service.joinThread('thread1');
      expect(mocks.mockHubConnection.invoke).toHaveBeenCalledWith('JoinThread', 'thread1');
    });

    it('should leave thread if connected', () => {
      service.startConnection('token');
      service.leaveThread('thread1');
      expect(mocks.mockHubConnection.invoke).toHaveBeenCalledWith('LeaveThread', 'thread1');
    });
  });
});
