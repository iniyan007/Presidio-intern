import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-operator-buses',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './buses.html',
  styleUrl: './buses.css'
})
export class BusesComponent implements OnInit {
  api = inject(ApiService);
  fb = inject(FormBuilder);

  buses: any[] = [];
  showModal = false;
  
  busForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    busNumber: ['', Validators.required],
    totalSeats: [40, [Validators.required, Validators.min(10)]],
    price: [1000, [Validators.required, Validators.min(100)]]
  });

  ngOnInit() {
    this.fetchBuses();
  }

  fetchBuses() {
    this.api.getOperatorBuses().subscribe(res => {
      this.buses = res;
    });
  }

  onSubmit() {
    if (this.busForm.valid) {
      this.api.addBus(this.busForm.value).subscribe(() => {
        this.fetchBuses();
        this.showModal = false;
        this.busForm.reset({totalSeats: 40, price: 1000});
      });
    }
  }

  deleteBus(id: number) {
    if(confirm('Delete this bus?')) {
      this.api.deleteBus(id).subscribe(() => {
        this.buses = this.buses.filter(b => b.id !== id);
      });
    }
  }
}
