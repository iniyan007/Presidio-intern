import { Component, signal } from '@angular/core';
import { Customers } from './customers/customers';
import { Cards } from './cards/cards';

@Component({
  standalone: true,
  selector: 'app-root',
  imports: [Customers, Cards],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('first_app');
}
