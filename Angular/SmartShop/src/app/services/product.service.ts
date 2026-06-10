import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, tap } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService{
  constructor(private http: HttpClient) {}

  public getProductsFromDummyJson(){
    return this.http.get('https://dummyjson.com/products').pipe(
      tap(() => console.log('ProductApiService: fetched products')),
      map((response) => response),
      catchError((error) => {
        console.error('ProductApiService: failed to load products', error);
        return throwError(() => error);
      })
    );
  }

  getProductById(id: number) {
    return this.http.get(`https://dummyjson.com/products/${id}`).pipe(
      tap(() => console.log(`ProductApiService: fetched product ${id}`)),
      map((response) => response),
      catchError((error) => {
        console.error(`ProductApiService: failed to load product ${id}`, error);
        return throwError(() => error);
      })
    );
  }
}