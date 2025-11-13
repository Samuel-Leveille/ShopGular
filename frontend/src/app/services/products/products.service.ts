import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Product } from '../../model/productInterface';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ProductSummary } from '../../model/product-summary';
import { CategoryProducts } from '../../model/category-products';
import { PRODUCT_TAG_OPTIONS, ProductTagOption, TagSeverity } from '../../model/product-tag-options';
import { PagedProducts } from '../../model/paged-products';
import { ProductDetail } from '../../model/product-detail';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {

  private readonly api = environment.apiBaseUrl;
  private readonly backendOrigin = new URL(environment.apiBaseUrl).origin;
  private readonly tagOptions = PRODUCT_TAG_OPTIONS;

  constructor(private http: HttpClient) { }

  getProducts() {
    return this.http.get<Product[]>('https://fakestoreapi.com/products');
  }

  createSellerProduct(formData: FormData): Observable<ProductSummary> {
    return this.http.post<any>(`${this.api}seller/products`, formData).pipe(
      map(created => this.normalizeProduct(created))
    );
  }

  updateSellerProduct(productId: number, formData: FormData): Observable<ProductSummary> {
    return this.http.put<any>(`${this.api}seller/products/${productId}`, formData).pipe(
      map(updated => this.normalizeProduct(updated))
    );
  }

  getSellerProducts(): Observable<ProductSummary[]> {
    return this.http.get<any[]>(`${this.api}seller/products`).pipe(
      map(items => items.map(item => this.normalizeProduct(item)))
    );
  }

  getProductsByCategory(limit = 4): Observable<CategoryProducts[]> {
    const params = { limit: `${limit}` };
    return this.http.get<any[]>(`${this.api}product/by-category`, { params }).pipe(
      map(blocks => (Array.isArray(blocks) ? blocks : [])
        .map(block => ({
          category: this.toString(block?.category),
          products: Array.isArray(block?.products)
            ? block.products.map((raw: any) => this.normalizeProduct(raw))
            : []
        }))
        .filter(block => block.category && block.products.length > 0)
      )
    );
  }

  normalizeProduct(raw: any): ProductSummary {
    const tag = this.toNumber(raw?.tag ?? raw?.Tag);
    const tagMeta = this.resolveTagMeta(tag);

    return {
      id: this.toNumber(raw?.id ?? raw?.Id),
      title: this.toString(raw?.title ?? raw?.Title),
      description: this.toString(raw?.description ?? raw?.Description),
      price: this.toNumber(raw?.price ?? raw?.Price),
      category: this.toString(raw?.category ?? raw?.Category),
      image: this.resolveImage(raw?.image ?? raw?.Image),
      quantity: this.toNumber(raw?.quantity ?? raw?.Quantity),
      purchaseQuantity: this.toNumber(raw?.purchaseQuantity ?? raw?.PurchaseQuantity),
      tag,
      tagLabel: tagMeta.label,
      tagSeverity: tagMeta.severity,
      dateOfSale: this.toIsoDate(raw?.dateOfSale ?? raw?.DateOfSale),
      sellerId: this.toNumber(raw?.sellerId ?? raw?.SellerId)
    };
  }

  normalizeProductDetail(raw: any): ProductDetail {
    const base = this.normalizeProduct(raw);
    const sellerName = this.toString(raw?.sellerName ?? raw?.SellerName);
    return {
      ...base,
      sellerId: this.toNumber(raw?.sellerId ?? raw?.SellerId ?? base.sellerId),
      sellerName: sellerName || 'Entreprise',
      sellerEmail: this.toString(raw?.sellerEmail ?? raw?.SellerEmail)
    };
  }

  private resolveTagMeta(value: number): { label: string; severity: TagSeverity } {
    const option: ProductTagOption | undefined = this.tagOptions.find(opt => opt.value === value);
    if (option) {
      return { label: option.label, severity: option.severity };
    }
    return { label: 'N/A', severity: 'info' };
  }

  private resolveImage(value: any): string {
    const str = this.toString(value);
    if (!str) {
      return '';
    }
    if (/^https?:\/\//i.test(str)) {
      return str;
    }
    if (str.startsWith('/')) {
      return `${this.backendOrigin}${str}`;
    }
    return `${this.backendOrigin}/${str}`.replace(/([^:]\/)\/+/g, '$1');
  }

  private toNumber(value: any, fallback = 0): number {
    const numeric = typeof value === 'number' ? value : Number(value);
    return Number.isFinite(numeric) ? numeric : fallback;
  }

  private toString(value: any, fallback = ''): string {
    if (value == null) {
      return fallback;
    }
    return String(value);
  }

  private toIsoDate(value: any): string {
    if (!value) {
      return '';
    }
    const date = value instanceof Date
      ? value
      : typeof value === 'string'
        ? new Date(value)
        : new Date(Number(value));

    if (Number.isNaN(date.getTime())) {
      return '';
    }

    return date.toISOString();
  }

  searchProducts(query: string, page = 1, pageSize = 10): Observable<PagedProducts> {
    const params = {
      q: query,
      page: `${page}`,
      pageSize: `${pageSize}`
    };
    return this.http.get<any>(`${this.api}product/search`, { params }).pipe(
      map(response => {
        const items = Array.isArray(response?.items)
          ? response.items.map((raw: any) => this.normalizeProduct(raw))
          : [];
        return {
          items,
          totalCount: this.toNumber(response?.totalCount, 0),
          page: this.toNumber(response?.page, page),
          pageSize: this.toNumber(response?.pageSize, pageSize)
        };
      })
    );
  }

  getProductDetail(productId: number): Observable<ProductDetail> {
    return this.http.get<any>(`${this.api}product/${productId}`).pipe(
      map(detail => this.normalizeProductDetail(detail))
    );
  }
}
