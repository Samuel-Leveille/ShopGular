import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../services/auth/auth.service';
import { ProductsService } from '../../services/products/products.service';
import { CreateProductPayload } from '../../model/create-product-payload';
import { PRODUCT_TAG_OPTIONS, TagSeverity } from '../../model/product-tag-options';
import { ProductSummary } from '../../model/product-summary';

@Component({
  selector: 'app-new-product',
  standalone: true,
  providers: [MessageService],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    InputTextareaModule,
    InputNumberModule,
    DropdownModule,
    ButtonModule,
    CardModule,
    ToastModule,
    TagModule
  ],
  templateUrl: './new-product.component.html',
  styleUrl: './new-product.component.css'
})
export class NewProductComponent implements OnInit {

  form: FormGroup;
  loading = false;
  products: ProductSummary[] = [];
  readonly tagOptions = PRODUCT_TAG_OPTIONS;
  readonly categories = [
    { label: 'Technologie', value: 'Technologie' },
    { label: 'Mode', value: 'Mode' },
    { label: 'Maison', value: 'Maison' },
    { label: 'Beauté', value: 'Beauté' },
    { label: 'Sport', value: 'Sport' },
    { label: 'Autre', value: 'Autre' }
  ];

  isSeller = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly productsService: ProductsService,
    private readonly messageService: MessageService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(120)]],
      description: ['', [Validators.required, Validators.maxLength(1000)]],
      price: [null, [Validators.required, Validators.min(0.01)]],
      category: [null, Validators.required],
      quantity: [1, [Validators.required, Validators.min(0)]],
      image: [''],
      tag: [this.tagOptions[0].value, Validators.required]
    });
  }

  ngOnInit(): void {
    this.auth.currentUser$.subscribe((user) => {
      this.isSeller = user?.type === 'seller';
      if (this.isSeller) {
        this.loadProducts();
      }
    });
  }

  submit(): void {
    if (!this.isSeller) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Action non disponible',
        detail: 'Seuls les vendeurs peuvent créer un produit.'
      });
      this.router.navigate(['/']);
      return;
    }

    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    const value = this.form.value;
    const payload: CreateProductPayload = {
      title: value.title,
      description: value.description,
      price: Number(value.price),
      category: value.category,
      image: value.image || 'https://via.placeholder.com/600x400?text=ShopGular',
      quantity: Number(value.quantity),
      purchaseQuantity: 0,
      tag: Number(value.tag),
      dateOfSale: new Date().toISOString()
    };

    this.productsService.createSellerProduct(payload).subscribe({
      next: (created) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Produit créé',
          detail: `${payload.title} est désormais visible dans votre catalogue.`
        });
        this.form.reset({
          title: '',
          description: '',
          price: null,
          category: null,
          quantity: 1,
          image: '',
          tag: this.tagOptions[0].value
        });
        this.appendProduct(created);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Erreur',
          detail: 'Impossible de créer le produit. Veuillez réessayer.'
        });
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  severityForTag(tagValue: number): TagSeverity {
    return this.tagOptions.find(option => option.value === tagValue)?.severity ?? 'info';
  }

  labelForTag(tagValue: number): string {
    return this.tagOptions.find(option => option.value === tagValue)?.label ?? 'N/A';
  }

  private loadProducts(): void {
    this.productsService.getSellerProducts().subscribe({
      next: (items) => {
        this.products = items.map(item => this.toSummary(item));
      }
    });
  }

  private appendProduct(raw: any): void {
    const summary = this.toSummary(raw);
    this.products = [summary, ...this.products.filter(p => p.id !== summary.id)];
  }

  private toSummary(raw: any): ProductSummary {
    return {
      id: raw.id ?? raw.Id ?? 0,
      title: raw.title ?? raw.Title ?? '',
      description: raw.description ?? raw.Description ?? '',
      price: raw.price ?? raw.Price ?? 0,
      category: raw.category ?? raw.Category ?? '',
      image: raw.image ?? raw.Image ?? '',
      quantity: raw.quantity ?? raw.Quantity ?? 0,
      purchaseQuantity: raw.purchaseQuantity ?? raw.PurchaseQuantity ?? 0,
      tag: raw.tag ?? raw.Tag ?? 0,
      dateOfSale: raw.dateOfSale ?? raw.DateOfSale ?? new Date().toISOString(),
      sellerId: raw.sellerId ?? raw.SellerId ?? 0
    };
  }
}

