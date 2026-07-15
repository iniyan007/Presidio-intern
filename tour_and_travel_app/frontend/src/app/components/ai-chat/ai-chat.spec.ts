import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AiChatComponent } from './ai-chat';
import { AiService } from '../../services/ai.service';
import { AuthService } from '../../services/auth.service';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('AiChatComponent', () => {
  let component: AiChatComponent;
  let fixture: ComponentFixture<AiChatComponent>;
  let aiServiceSpy: any;
  let authServiceSpy: any;

  beforeEach(async () => {
    aiServiceSpy = {
      sendMessage: vi.fn()
    };
    authServiceSpy = {
      isAuthenticated: vi.fn().mockReturnValue(true)
    };

    await TestBed.configureTestingModule({
      imports: [AiChatComponent],
      providers: [
        { provide: AiService, useValue: aiServiceSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AiChatComponent);
    component = fixture.componentInstance;
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should toggle chat and scroll to bottom', () => {
    fixture.detectChanges();
    expect(component.isOpen()).toBeFalsy();

    const scrollSpy = vi.spyOn(component as any, 'scrollToBottom');
    
    component.toggleChat();
    expect(component.isOpen()).toBeTruthy();
    
    vi.advanceTimersByTime(100);
    expect(scrollSpy).toHaveBeenCalled();
  });

  it('should not send message if text is empty', () => {
    fixture.detectChanges();
    component.userInput.set('   ');
    component.sendMessage();
    expect(aiServiceSpy.sendMessage).not.toHaveBeenCalled();
  });

  it('should send message successfully', () => {
    fixture.detectChanges();
    component.userInput.set('Hello AI');
    
    const mockResponse = { success: true, data: { reply: 'Hello User' } };
    aiServiceSpy.sendMessage.mockReturnValue(of(mockResponse));

    component.sendMessage();

    expect(component.userInput()).toBe('');
    expect(component.isLoading()).toBeFalsy();
    
    const messages = component.messages();
    expect(messages.length).toBe(3); // 1 initial + 1 user + 1 ai
    expect(messages[1]).toEqual({ role: 'user', text: 'Hello AI' });
    expect(messages[2]).toEqual({ role: 'ai', text: 'Hello User' });
  });

  it('should handle error when sending message', () => {
    fixture.detectChanges();
    component.userInput.set('Hello AI');
    
    aiServiceSpy.sendMessage.mockReturnValue(throwError(() => new Error('Network error')));

    component.sendMessage();

    expect(component.userInput()).toBe('');
    expect(component.isLoading()).toBeFalsy();
    
    const messages = component.messages();
    expect(messages.length).toBe(3);
    expect(messages[2].text).toContain('trouble connecting');
  });

  it('should send message on Enter key press without shift', () => {
    fixture.detectChanges();
    const sendSpy = vi.spyOn(component, 'sendMessage');
    const event = new KeyboardEvent('keydown', { key: 'Enter', shiftKey: false });
    event.preventDefault = vi.fn();
    
    component.onKeyDown(event);
    
    expect(event.preventDefault).toHaveBeenCalled();
    expect(sendSpy).toHaveBeenCalled();
  });

  it('should not send message on Enter + Shift', () => {
    fixture.detectChanges();
    const sendSpy = vi.spyOn(component, 'sendMessage');
    const event = new KeyboardEvent('keydown', { key: 'Enter', shiftKey: true });
    
    component.onKeyDown(event);
    
    expect(sendSpy).not.toHaveBeenCalled();
  });
});
