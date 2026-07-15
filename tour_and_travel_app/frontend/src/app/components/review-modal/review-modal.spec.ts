import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReviewModalComponent } from './review-modal';
import { BookingService } from '../../services/booking.service';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('ReviewModalComponent', () => {
  let component: ReviewModalComponent;
  let fixture: ComponentFixture<ReviewModalComponent>;
  let bookingServiceSpy: any;

  beforeEach(async () => {
    bookingServiceSpy = {
      uploadReviewMedia: vi.fn(),
      createReview: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ReviewModalComponent],
      providers: [
        { provide: BookingService, useValue: bookingServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ReviewModalComponent);
    component = fixture.componentInstance;
    component.bookingId = 'test-booking-id';
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should set ratings correctly', () => {
    component.setRating('overall', 4);
    component.setRating('accommodation', 3);
    component.setRating('transport', 5);
    component.setRating('food', 2);
    component.setRating('guide', 4);
    component.setRating('value', 3);

    expect(component.overallRating()).toBe(4);
    expect(component.accommodationRating()).toBe(3);
    expect(component.transportRating()).toBe(5);
    expect(component.foodRating()).toBe(2);
    expect(component.guideRating()).toBe(4);
    expect(component.valueRating()).toBe(3);
  });

  it('should handle file selection', () => {
    const file1 = new File(['test1'], 'test1.jpg', { type: 'image/jpeg' });
    const file2 = new File(['test2'], 'test2.jpg', { type: 'image/jpeg' });
    const event = {
      target: {
        files: [file1, file2]
      }
    } as unknown as Event;

    // mock FileReader
    const mockFileReader = {
      readAsDataURL: vi.fn(function(this: any, file: File) {
        this.onload({ target: { result: `data:image/jpeg;base64,mock_${file.name}` } });
      }),
    };
    vi.stubGlobal('FileReader', vi.fn(function() { return mockFileReader; }));

    component.onFileSelected(event);

    expect(component.selectedFiles().length).toBe(2);
    expect(component.previewUrls().length).toBe(2);
    expect(component.previewUrls()[0]).toBe('data:image/jpeg;base64,mock_test1.jpg');
    expect(component.previewUrls()[1]).toBe('data:image/jpeg;base64,mock_test2.jpg');
  });

  it('should remove file correctly', () => {
    component.selectedFiles.set([new File([], '1'), new File([], '2')]);
    component.previewUrls.set(['url1', 'url2']);

    component.removeFile(0);

    expect(component.selectedFiles().length).toBe(1);
    expect(component.previewUrls().length).toBe(1);
    expect(component.previewUrls()[0]).toBe('url2');
  });

  it('should submit review without media files', () => {
    const submittedSpy = vi.spyOn(component.submitted, 'emit');
    bookingServiceSpy.createReview.mockReturnValue(of({ success: true }));

    component.overallRating.set(4);
    component.comment.set('Good!');
    component.submitReview();

    expect(component.isSubmitting()).toBeFalsy();
    expect(bookingServiceSpy.uploadReviewMedia).not.toHaveBeenCalled();
    expect(bookingServiceSpy.createReview).toHaveBeenCalledWith('test-booking-id', {
      bookingId: 'test-booking-id',
      overallRating: 4,
      accommodationRating: 5,
      transportRating: 5,
      foodRating: 5,
      guideRating: 5,
      valueRating: 5,
      comment: 'Good!',
      mediaFilePaths: []
    });
    expect(submittedSpy).toHaveBeenCalled();
  });

  it('should submit review with media files', () => {
    const submittedSpy = vi.spyOn(component.submitted, 'emit');
    const mockFile = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
    component.selectedFiles.set([mockFile]);
    
    bookingServiceSpy.uploadReviewMedia.mockReturnValue(of({ paths: ['path/to/media.jpg'] }));
    bookingServiceSpy.createReview.mockReturnValue(of({ success: true }));

    component.submitReview();

    expect(bookingServiceSpy.uploadReviewMedia).toHaveBeenCalledWith([mockFile]);
    expect(bookingServiceSpy.createReview).toHaveBeenCalledWith('test-booking-id', expect.objectContaining({
      mediaFilePaths: ['path/to/media.jpg']
    }));
    expect(submittedSpy).toHaveBeenCalled();
  });

  it('should handle error when uploading media fails', () => {
    const mockFile = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
    component.selectedFiles.set([mockFile]);
    
    bookingServiceSpy.uploadReviewMedia.mockReturnValue(throwError(() => ({ error: { message: 'Upload Error' } })));

    component.submitReview();

    expect(bookingServiceSpy.uploadReviewMedia).toHaveBeenCalled();
    expect(bookingServiceSpy.createReview).not.toHaveBeenCalled();
    expect(component.isSubmitting()).toBeFalsy();
    expect(component.errorMessage()).toBe('Upload Error');
  });

  it('should handle error when creating review fails', () => {
    bookingServiceSpy.createReview.mockReturnValue(throwError(() => ({ error: { message: 'Create Error' } })));

    component.submitReview();

    expect(bookingServiceSpy.createReview).toHaveBeenCalled();
    expect(component.isSubmitting()).toBeFalsy();
    expect(component.errorMessage()).toBe('Create Error');
  });
});
