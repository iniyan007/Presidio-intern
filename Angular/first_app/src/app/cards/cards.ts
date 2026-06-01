import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductModel } from '../models/product.model';

@Component({
  standalone: true,
  selector: 'app-cards',
  imports: [CommonModule],
  templateUrl: './cards.html',
  styleUrls: ['./cards.css'],
})
export class Cards {
  products: ProductModel[] = [
    new ProductModel(
      'Elegant Leather Wallet',
      1499,
      'Handcrafted leather wallet with multiple card slots and a sleek finish.',
      'https://via.placeholder.com/400x250?text=Leather+Wallet'
    ),
    new ProductModel(
      'Wireless Headphones',
      5999,
      'Noise-cancelling wireless headphones with long battery life and crisp sound.',
      'https://via.placeholder.com/400x250?text=Headphones'
    ),
    new ProductModel(
      'Modern Desk Lamp',
      2499,
      'Adjustable LED desk lamp with touch controls and warm/cool light modes.',
      'https://via.placeholder.com/400x250?text=Desk+Lamp'
    ),
    new ProductModel(
      'Stylish Backpack',
      3499,
      'Durable and spacious backpack with multiple compartments and a sleek design.',
      'https://via.placeholder.com/400x250?text=Backpack'
    ),
    new ProductModel(
      'Smartwatch Pro',
      8999,
      'Feature-packed smartwatch with fitness tracking, notifications, and customizable watch faces.',
      'https://via.placeholder.com/400x250?text=Smartwatch'
    ),
  ];
}
