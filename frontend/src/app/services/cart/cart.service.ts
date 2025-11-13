import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { CartItem } from '../../model/cart-item';
import { ProductSummary } from '../../model/product-summary';
import { ProductDetail } from '../../model/product-detail';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  private readonly storageKey = 'sg.cart';
  private readonly itemsSubject = new BehaviorSubject<CartItem[]>(this.loadFromStorage());
  readonly items$ = this.itemsSubject.asObservable();
  readonly total$ = this.items$.pipe(map(items => this.computeTotal(items)));

  addProduct(product: ProductSummary | ProductDetail, quantity = 1): void {
    if (!product || quantity <= 0) {
      return;
    }

    const current = this.itemsSubject.value;
    const existing = current.find(item => item.id === product.id);
    const availableStock = Number((product as any).quantity ?? 0);
    const sanitizedQuantity = Math.min(quantity, availableStock);
    if (sanitizedQuantity <= 0) {
      return;
    }

    let updated: CartItem[];

    if (existing) {
      const newQuantity = Math.min(existing.quantity + sanitizedQuantity, availableStock);
      updated = current.map(item =>
        item.id === existing.id
          ? { ...item, quantity: newQuantity }
          : item
      );
    } else {
      const newItem: CartItem = {
        id: product.id,
        title: product.title,
        price: product.price,
        image: product.image,
        quantity: sanitizedQuantity,
        stock: availableStock,
        sellerId: product.sellerId ?? 0
      };
      updated = [...current, newItem];
    }

    this.updateState(updated);
  }

  updateQuantity(productId: number, quantity: number): void {
    if (quantity < 0) {
      quantity = 0;
    }

    const updated = this.itemsSubject.value
      .map(item => item.id === productId
        ? { ...item, quantity: Math.min(quantity, item.stock) }
        : item
      )
      .filter(item => item.quantity > 0);

    this.updateState(updated);
  }

  increment(productId: number): void {
    const item = this.itemsSubject.value.find(i => i.id === productId);
    if (!item) {
      return;
    }
    this.updateQuantity(productId, Math.min(item.quantity + 1, item.stock));
  }

  decrement(productId: number): void {
    const item = this.itemsSubject.value.find(i => i.id === productId);
    if (!item) {
      return;
    }
    this.updateQuantity(productId, item.quantity - 1);
  }

  remove(productId: number): void {
    const updated = this.itemsSubject.value.filter(item => item.id !== productId);
    this.updateState(updated);
  }

  clear(): void {
    this.updateState([]);
  }

  getTotal(): number {
    return this.computeTotal(this.itemsSubject.value);
  }

  getItemsSnapshot(): CartItem[] {
    return [...this.itemsSubject.value];
  }

  private computeTotal(items: CartItem[]): number {
    return items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  private updateState(items: CartItem[]): void {
    this.itemsSubject.next(items);
    this.persist(items);
  }

  private loadFromStorage(): CartItem[] {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) {
        return [];
      }
      const parsed = JSON.parse(raw);
      if (!Array.isArray(parsed)) {
        return [];
      }
      return parsed
        .map(item => ({
          id: Number(item.id),
          title: String(item.title ?? ''),
          price: Number(item.price ?? 0),
          image: String(item.image ?? ''),
          quantity: Number(item.quantity ?? 0),
          stock: Number(item.stock ?? 0),
          sellerId: Number(item.sellerId ?? 0)
        }))
        .filter(item => Number.isFinite(item.id) && item.quantity > 0 && item.price >= 0);
    } catch {
      return [];
    }
  }

  private persist(items: CartItem[]): void {
    try {
      localStorage.setItem(this.storageKey, JSON.stringify(items));
    } catch {
      // ignore
    }
  }
}

