import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Product } from '../../model/productInterface';
import { environment } from '../../../environments/environment';
import { CreateProductPayload } from '../../model/create-product-payload';
import { Observable } from 'rxjs';
import { ProductSummary } from '../../model/product-summary';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {

  private readonly api = environment.apiBaseUrl;

  constructor(private http: HttpClient) { }

  getProducts() {
    return this.http.get<Product[]>('https://fakestoreapi.com/products');
  }

  createSellerProduct(payload: CreateProductPayload): Observable<ProductSummary> {
    return this.http.post<ProductSummary>(`${this.api}seller/products`, payload);
  }

  getSellerProducts(): Observable<ProductSummary[]> {
    return this.http.get<ProductSummary[]>(`${this.api}seller/products`);
  }
}
