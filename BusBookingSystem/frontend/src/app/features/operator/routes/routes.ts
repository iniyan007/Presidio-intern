import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-routes',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './routes.html',
  styleUrl: './routes.css'
})
export class RoutesComponent implements OnInit {
  api = inject(ApiService);
  fb = inject(FormBuilder);

  routes: any[] = [];
  showModal = false;
  
  routeForm: FormGroup = this.fb.group({
    source: ['', Validators.required],
    destination: ['', Validators.required]
  });

  ngOnInit() {
    this.fetchRoutes();
  }

  fetchRoutes() {
    this.api.getRoutes().subscribe(res => {
      this.routes = res;
    });
  }

  onSubmit() {
    if (this.routeForm.valid) {
      this.api.createRoute(this.routeForm.value).subscribe(() => {
        this.fetchRoutes();
        this.showModal = false;
        this.routeForm.reset();
      });
    }
  }

  deleteRoute(id: number) {
    if(confirm('Delete this route?')) {
      this.api.deleteRoute(id).subscribe(() => {
        this.routes = this.routes.filter(r => r.id !== id);
      });
    }
  }
}
