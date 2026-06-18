import { Component, EventEmitter, Input, Output, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BookingService } from '../../services/booking.service';

@Component({
  selector: 'app-review-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './review-modal.html'
})
export class ReviewModalComponent {
  @Input() bookingId!: string;
  @Output() close = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<void>();

  private bookingService = inject(BookingService);

  overallRating = signal<number>(5);
  accommodationRating = signal<number>(5);
  transportRating = signal<number>(5);
  foodRating = signal<number>(5);
  guideRating = signal<number>(5);
  valueRating = signal<number>(5);
  comment = signal<string>('');

  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string>('');
  
  selectedFiles = signal<File[]>([]);
  previewUrls = signal<string[]>([]);

  setRating(category: string, value: number) {
    switch (category) {
      case 'overall': this.overallRating.set(value); break;
      case 'accommodation': this.accommodationRating.set(value); break;
      case 'transport': this.transportRating.set(value); break;
      case 'food': this.foodRating.set(value); break;
      case 'guide': this.guideRating.set(value); break;
      case 'value': this.valueRating.set(value); break;
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const newFiles = Array.from(input.files);
      this.selectedFiles.update(files => [...files, ...newFiles]);
      
      newFiles.forEach(file => {
        const reader = new FileReader();
        reader.onload = (e) => {
          this.previewUrls.update(urls => [...urls, e.target?.result as string]);
        };
        reader.readAsDataURL(file);
      });
    }
  }

  removeFile(index: number) {
    this.selectedFiles.update(files => files.filter((_, i) => i !== index));
    this.previewUrls.update(urls => urls.filter((_, i) => i !== index));
  }

  submitReview() {
    this.isSubmitting.set(true);
    this.errorMessage.set('');

    if (this.selectedFiles().length > 0) {
      this.bookingService.uploadReviewMedia(this.selectedFiles()).subscribe({
        next: (res) => {
          this.submitReviewPayload(res.paths);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(err.error?.message || 'Failed to upload images.');
        }
      });
    } else {
      this.submitReviewPayload([]);
    }
  }

  private submitReviewPayload(mediaPaths: string[]) {
    const payload = {
      bookingId: this.bookingId,
      overallRating: this.overallRating(),
      accommodationRating: this.accommodationRating(),
      transportRating: this.transportRating(),
      foodRating: this.foodRating(),
      guideRating: this.guideRating(),
      valueRating: this.valueRating(),
      comment: this.comment(),
      mediaFilePaths: mediaPaths
    };

    this.bookingService.createReview(this.bookingId, payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.submitted.emit();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to submit review.');
      }
    });
  }
}
