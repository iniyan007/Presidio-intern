
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService {
    constructor(private httpClient: HttpClient) {}
    public getProductsFromDummyJson() {
        return this.httpClient.get("https://dummyjson.com/products");
    } 
}