import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { CartService } from '../../services/cart/cart.service';
import { CartItem } from '../../model/cart-item';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, ButtonModule, ToastModule],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
  providers: [MessageService],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CartComponent implements OnInit, OnDestroy {
  items: CartItem[] = [];
  total = 0;
  isClient = false;

  private subscriptions: Subscription[] = [];

  constructor(
    private readonly cart: CartService,
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly messageService: MessageService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const userSub = this.auth.currentUser$.subscribe(user => {
      const type = user?.type;
      this.isClient = type === 'client';
      if (!this.isClient) {
        this.items = [];
        this.total = 0;
        this.cdr.markForCheck();
      }
    });

    const cartSub = this.cart.items$.subscribe(items => {
      this.items = items;
      this.total = this.cart.getTotal();
      this.cdr.markForCheck();
    });

    this.subscriptions.push(userSub, cartSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  increase(item: CartItem): void {
    if (item.quantity >= item.stock) {
      this.messageService.add({ severity: 'warn', summary: 'Stock limité', detail: 'Stock maximal atteint pour ce produit.' });
      return;
    }
    this.cart.increment(item.id);
  }

  decrease(item: CartItem): void {
    this.cart.decrement(item.id);
  }

  remove(item: CartItem): void {
    this.cart.remove(item.id);
    this.messageService.add({ severity: 'info', summary: 'Produit retiré', detail: `${item.title} a été retiré du panier.` });
  }

  goToHome(): void {
    this.router.navigate(['/']);
  }
}

