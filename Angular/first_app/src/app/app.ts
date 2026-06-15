import { Component, signal } from '@angular/core';
import { Customers } from './customers/customers';
import { Products } from './product/product';

@Component({
  standalone: true,
  selector: 'app-root',
  imports: [Customers, Products],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('first_app');
}
