import { Component, input, output, signal } from '@angular/core';
import { ProductModel } from '../../models/product.model';
import { ActivatedRoute } from '@angular/router';
import { ProductApiService } from '../../services/product.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-product-details',
  imports: [],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
})
export class ProductDetails {

  constructor(private route: ActivatedRoute, private productApiService: ProductApiService, private router: Router){

  }
  product = signal<ProductModel | null>(null);
  ngOnInit() {

    const id = Number(
      this.route.snapshot.paramMap.get('id')
    );

    this.productApiService
      .getProductById(id)
      .subscribe(product => {
        this.product.set(product as ProductModel);
      });
  }

    goBack() {
      this.router.navigate(['/products']);
    }
}
