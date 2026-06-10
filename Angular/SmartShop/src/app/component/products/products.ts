import { Component, signal } from '@angular/core';
import { ProductModel } from '../../models/product.model';
import { ProductApiService } from '../../services/product.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-products',
  imports: [],
  templateUrl:'./products.html',
  styleUrl: './products.css',
})
export class Products {

  products = signal<ProductModel[]>([]);
  
  constructor(private productApiService: ProductApiService,  private router: Router) {
    this.productApiService.getProductsFromDummyJson()
      .subscribe({
      next:(response: any) => {
        this.products.set(response.products);
      },
      error:(error) => {
        console.error(error);
      },
      complete:()=>{
        
      }
    });
  }
  viewDetails(id: number) {
    this.router.navigate(['/products', id]);
  }
}
