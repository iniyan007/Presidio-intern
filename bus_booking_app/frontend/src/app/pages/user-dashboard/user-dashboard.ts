import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { BookingService } from '../../services/booking';
import { User } from '../../models/user.model';
import { RouterLink } from '@angular/router';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-user-dashboard',
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './user-dashboard.html',
  styleUrl: './user-dashboard.css'
})
export class UserDashboard implements OnInit {
  activeTab: string = 'bookings'; // 'bookings' or 'profile'
  user: User | null = null;
  bookings: any[] = [];
  
  isEditing: boolean = false;
  profileForm!: FormGroup;
  message: string = '';

  constructor(
    private authService: AuthService,
    private bookingService: BookingService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder
  ) {}

  ngOnInit() {
    this.user = this.authService.currentUserValue;
    this.initProfileForm();
    this.loadBookings();
  }

  initProfileForm() {
    this.profileForm = this.fb.group({
      name: [this.user?.name || '', Validators.required],
      mobileNumber: [this.user?.mobileNumber || '', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      age: [this.user?.age || '', [Validators.required, Validators.min(1)]],
      gender: [this.user?.gender || '', Validators.required]
    });
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    if (this.isEditing) {
      this.initProfileForm(); // Reset form to current user values
    }
  }

  onUpdateProfile() {
    if (this.profileForm.invalid) return;

    this.authService.updateProfile(this.profileForm.value).subscribe({
      next: (res) => {
        this.user = res.user;
        this.message = 'Profile updated successfully!';
        this.isEditing = false;
        this.cdr.detectChanges();
        setTimeout(() => this.message = '', 3000);
      },
      error: (err) => {
        this.message = err.error?.message || 'Error updating profile';
        this.cdr.detectChanges();
        setTimeout(() => this.message = '', 3000);
      }
    });
  }

  loadBookings() {
    this.bookingService.getMyBookings().subscribe({
      next: (res) => {
        this.bookings = res;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error fetching bookings:', err)
    });
  }

  downloadTicket(booking: any) {
    const doc = new jsPDF();
    const dateStr = new Date(booking.trip.departureTime).toLocaleString();
    const bookedAtStr = new Date(booking.createdAt).toLocaleString();

    // Header
    doc.setFontSize(22);
    doc.setTextColor(37, 99, 235); // var(--primary-color)
    doc.text('Bus Booking Ticket', 105, 20, { align: 'center' });

    // Status Badge
    doc.setFontSize(12);
    doc.setTextColor(34, 197, 94); // var(--success-color)
    doc.text('CONFIRMED', 105, 30, { align: 'center' });

    // Subtitle / Date booked
    doc.setFontSize(10);
    doc.setTextColor(100, 116, 139);
    doc.text(`Booked On: ${bookedAtStr}`, 105, 38, { align: 'center' });

    // Content
    doc.setTextColor(0, 0, 0);
    doc.setFontSize(14);
    
    // Route Info
    doc.text(`Journey: ${booking.trip.route.source} to ${booking.trip.route.destination}`, 15, 55);
    
    // Bus Info
    doc.setFontSize(12);
    doc.text(`Bus Name: ${booking.trip.bus.name}`, 15, 65);
    doc.text(`Bus Number: ${booking.trip.bus.busNumber}`, 15, 75);
    doc.text(`Departure: ${dateStr}`, 15, 85);
    
    // Passenger Details Table
    const tableData = booking.seats.map((seat: any) => {
      // Fallback if seat is just a string (old cached data before refresh)
      if (typeof seat === 'string') {
        return [seat, 'N/A', 'N/A', 'N/A'];
      }
      return [
        seat.seatNumber || 'N/A',
        seat.passengerName || 'N/A',
        seat.passengerAge || 'N/A',
        seat.passengerGender || 'N/A'
      ];
    });

    autoTable(doc, {
      startY: 95,
      head: [['Seat No.', 'Passenger Name', 'Age', 'Gender']],
      body: tableData,
      theme: 'striped',
      headStyles: { fillColor: [37, 99, 235] },
      styles: { fontSize: 10 }
    });

    // Get the Y position after the table
    const finalY = (doc as any).lastAutoTable.finalY || 95;

    doc.setFontSize(14);
    doc.text(`Total Amount Paid: Rs. ${booking.totalAmount}`, 15, finalY + 15);

    // Disclaimer
    doc.setFontSize(10);
    doc.setTextColor(100, 116, 139);
    doc.text('Please carry this ticket and a valid ID proof during the journey.', 105, finalY + 30, { align: 'center' });
    doc.text('Happy Journey!', 105, finalY + 38, { align: 'center' });

    doc.save(`Ticket_${booking.trip.route.source}_to_${booking.trip.route.destination}.pdf`);
  }
}
