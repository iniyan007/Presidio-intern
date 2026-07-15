import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AiService, ChatMessage, ChatResponse } from './ai.service';
import { environment } from '../../environments/environment';

describe('AiService', () => {
  let service: AiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AiService]
    });
    service = TestBed.inject(AiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should send a message to AI and return response', () => {
    const mockMessages: ChatMessage[] = [{ role: 'user', text: 'Hello' }];
    const mockResponse: ChatResponse = {
      success: true,
      data: { reply: 'Hi there!' }
    };

    service.sendMessage(mockMessages).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Ai/chat`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ messages: mockMessages });
    req.flush(mockResponse);
  });
});
