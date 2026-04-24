import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../../../core/services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-operators',
  imports: [CommonModule],
  templateUrl: './operators.html',
  styleUrl: './operators.css'
})
export class OperatorsComponent implements OnInit {
  api = inject(ApiService);
  operators: any[] = [];
  loading = true;

  ngOnInit() {
    this.fetchOperators();
  }

  fetchOperators() {
    this.loading = true;
    this.api.getAllOperators().subscribe(res => {
      this.operators = res;
      this.loading = false;
    });
  }

  disableOperator(id: number) {
    if(confirm('Are you sure you want to disable this operator? All their future trips will be cancelled and users refunded.')) {
      this.api.disableOperator(id).subscribe(() => {
        alert('Operator disabled successfully');
        this.fetchOperators();
      });
    }
  }
}
