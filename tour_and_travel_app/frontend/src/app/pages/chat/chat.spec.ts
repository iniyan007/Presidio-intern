import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ChatComponent } from './chat';
import { ChatService } from '../../services/chat.service';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { ActivatedRoute } from '@angular/router';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { vi } from 'vitest';

describe('ChatComponent', () => {
  let component: ChatComponent;
  let fixture: ComponentFixture<ChatComponent>;

  let chatServiceSpy: any;
  let userServiceSpy: any;
  let authServiceSpy: any;
  let routeQueryParams: BehaviorSubject<any>;

  beforeEach(async () => {
    vi.useFakeTimers();
    // Spy on localStorage
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation((key) => {
      if (key === 'jwt_token') return 'fake-token';
      return null;
    });

    chatServiceSpy = {
      threads: signal([{ id: 't1', unreadCount: 5, userName: 'User', packagerName: 'Packager' }]),
      currentThreadMessages: signal([]),
      activeThreadId: signal(null),
      startConnection: vi.fn(),
      stopConnection: vi.fn(),
      getThreads: vi.fn().mockReturnValue(of([{ id: 't1', unreadCount: 5, userName: 'User', packagerName: 'Packager' }])),
      getThreadMessages: vi.fn().mockReturnValue(of([{ id: 'm1', body: 'Hello' }])),
      joinThread: vi.fn(),
      leaveThread: vi.fn(),
      markAsRead: vi.fn().mockReturnValue(of({})),
      sendMessage: vi.fn().mockReturnValue(of({ id: 'm2', body: 'Hi' }))
    };

    userServiceSpy = {
      userProfile: signal(null)
    };
    
    authServiceSpy = {
      getUserRole: vi.fn().mockReturnValue('User')
    };

    routeQueryParams = new BehaviorSubject({});

    await TestBed.configureTestingModule({
      imports: [ChatComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ChatService, useValue: chatServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: ActivatedRoute, useValue: { queryParams: routeQueryParams.asObservable() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ChatComponent);
    component = fixture.componentInstance;
    
    // Mock scroll container
    component['myScrollContainer'] = {
      nativeElement: { scrollTop: 0, scrollHeight: 100 }
    } as any;
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  it('should initialize and start connection when user profile is set', () => {
    userServiceSpy.userProfile.set({ id: 'u1', fullName: 'User 1' });
    fixture.detectChanges();
    
    expect(authServiceSpy.getUserRole).toHaveBeenCalled();
    expect(chatServiceSpy.startConnection).toHaveBeenCalledWith('fake-token');
    expect(chatServiceSpy.getThreads).toHaveBeenCalled();
  });

  it('should auto-select thread from query params', () => {
    routeQueryParams.next({ threadId: 't2' });
    fixture.detectChanges();
    
    vi.advanceTimersByTime(500); // Wait for the timeout in ngOnInit
    
    expect(component.activeThreadId()).toBe('t2');
    expect(chatServiceSpy.joinThread).toHaveBeenCalledWith('t2');
  });

  it('should cleanup on destroy', () => {
    component.activeThreadId.set('t1');
    fixture.detectChanges();
    
    component.ngOnDestroy();
    
    expect(chatServiceSpy.leaveThread).toHaveBeenCalledWith('t1');
    expect(chatServiceSpy.stopConnection).toHaveBeenCalled();
  });

  it('should select thread, load messages, and mark as read', () => {
    chatServiceSpy.getThreads.mockReturnValue(of([
      { id: 't1', unreadCount: 5, userName: 'User', packagerName: 'Packager' },
      { id: 't-old', unreadCount: 0, userName: 'Old User', packagerName: 'Old Packager' }
    ]));
    component.activeThreadId.set('t-old');
    fixture.detectChanges();
    
    component.selectThread('t1');
    
    expect(chatServiceSpy.leaveThread).toHaveBeenCalledWith('t-old');
    expect(chatServiceSpy.joinThread).toHaveBeenCalledWith('t1');
    expect(component.activeThreadId()).toBe('t1');
    expect(chatServiceSpy.getThreadMessages).toHaveBeenCalledWith('t1');
    expect(chatServiceSpy.markAsRead).toHaveBeenCalledWith('t1');
    
    // Check local unread count update
    expect(chatServiceSpy.threads()[0].unreadCount).toBe(0);
  });

  it('should not send message if empty or no active thread', () => {
    fixture.detectChanges();
    
    component.newMessage.set('   ');
    component.sendMessage();
    expect(chatServiceSpy.sendMessage).not.toHaveBeenCalled();
    
    component.newMessage.set('Hello');
    component.activeThreadId.set(null);
    component.sendMessage();
    expect(chatServiceSpy.sendMessage).not.toHaveBeenCalled();
  });

  it('should send message, clear input, and append to current messages', () => {
    authServiceSpy.getUserRole.mockReturnValue('Packager');
    userServiceSpy.userProfile.set({ id: 'u1', fullName: 'User 1' }); // Trigger role check effect
    fixture.detectChanges();
    
    const scrollToBottomSpy = vi.spyOn(component, 'scrollToBottom');

    component.activeThreadId.set('t1');
    component.newMessage.set('Hi there');
    
    component.sendMessage();
    
    expect(chatServiceSpy.sendMessage).toHaveBeenCalledWith({
      threadId: 't1',
      senderRole: 1, // Packager
      body: 'Hi there'
    });
    
    expect(component.newMessage()).toBe('');
    expect(chatServiceSpy.currentThreadMessages().length).toBe(1);
    expect(chatServiceSpy.currentThreadMessages()[0].id).toBe('m2');
    
    // Test scroll to bottom after send
    vi.advanceTimersByTime(100);
    expect(scrollToBottomSpy).toHaveBeenCalled();
  });

  it('should get correct thread title based on role', () => {
    fixture.detectChanges();
    const thread = { id: 't1', userName: 'User Bob', packagerName: 'Packager Alice' } as any;
    
    component.isPackager.set(true);
    expect(component.getThreadTitle(thread)).toBe('User Bob');
    
    component.isPackager.set(false);
    expect(component.getThreadTitle(thread)).toBe('Packager Alice');
  });

  it('should get active thread correctly', () => {
    fixture.detectChanges();
    component.activeThreadId.set('t1');
    expect(component.getActiveThread()?.id).toBe('t1');
  });

  it('should clear active thread', () => {
    fixture.detectChanges();
    component.activeThreadId.set('t1');
    component.clearActiveThread();
    expect(component.activeThreadId()).toBeNull();
  });

  it('should handle scrollToBottom gracefully when no container', () => {
    component['myScrollContainer'] = undefined as any;
    expect(() => component.scrollToBottom()).not.toThrow();
  });
});
