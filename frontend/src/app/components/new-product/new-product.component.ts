import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, NgZone, OnInit, ViewChild } from '@angular/core';
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
import { PRODUCT_TAG_OPTIONS, TagSeverity, ProductTagOption } from '../../model/product-tag-options';
import { ProductSummary } from '../../model/product-summary';
import { environment } from '../../../environments/environment';

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
  private readonly backendOrigin = new URL(environment.apiBaseUrl).origin;
  readonly categories = [
    { label: 'Technologie', value: 'Technologie' },
    { label: 'Mode', value: 'Mode' },
    { label: 'Maison', value: 'Maison' },
    { label: 'Beauté', value: 'Beauté' },
    { label: 'Sport', value: 'Sport' },
    { label: 'Autre', value: 'Autre' }
  ];

  isSeller = false;
  private selectedFile: File | null = null;
  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly productsService: ProductsService,
    private readonly messageService: MessageService,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(120)]],
      description: ['', [Validators.required, Validators.maxLength(1000)]],
      price: [null, [Validators.required, Validators.min(0.01)]],
      category: [null, Validators.required],
      quantity: [1, [Validators.required, Validators.min(0)]],
      image: [null, Validators.required],
      tag: [this.tagOptions[0].value, Validators.required]
    });
  }

  ngOnInit(): void {
    this.auth.currentUser$.subscribe((user) => {
      this.zone.run(() => {
        this.isSeller = user?.type === 'seller';
        if (this.isSeller) {
          this.loadProducts();
        } else {
          this.products = [];
        }
      });
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
    if (!this.selectedFile) {
      this.form.get('image')?.setErrors({ required: true });
      this.messageService.add({
        severity: 'warn',
        summary: 'Image requise',
        detail: 'Ajoutez un visuel avant de publier votre produit.'
      });
      return;
    }

    this.loading = true;
    const value = this.form.value;
    const price = Number(value.price);
    const quantity = Number(value.quantity);

    if (Number.isNaN(price) || price <= 0 || Number.isNaN(quantity) || quantity < 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Valeurs incorrectes',
        detail: 'Vérifiez le prix et la quantité saisis.'
      });
      this.loading = false;
      return;
    }

    const formData = new FormData();
    formData.append('Title', value.title);
    formData.append('Description', value.description);
    formData.append('Price', price.toString());
    formData.append('Category', value.category);
    formData.append('Quantity', quantity.toString());
    formData.append('PurchaseQuantity', '0');
    formData.append('Tag', String(value.tag));
    formData.append('DateOfSale', new Date().toISOString());
    if (this.selectedFile) {
      formData.append('ImageFile', this.selectedFile, this.selectedFile.name);
    }

    this.productsService.createSellerProduct(formData).subscribe({
      next: (created) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Produit créé',
          detail: `${value.title} est désormais visible dans votre catalogue.`
        });
        this.form.reset({
          title: '',
          description: '',
          price: null,
          category: null,
          quantity: 1,
          image: null,
          tag: this.tagOptions[0].value
        });
        this.selectedFile = null;
        if (this.fileInput?.nativeElement) {
          this.fileInput.nativeElement.value = '';
        }
        const imageControl = this.form.get('image');
        imageControl?.markAsPristine();
        imageControl?.markAsUntouched();
        imageControl?.updateValueAndValidity();
        this.appendProduct(created);
        this.cdr.detectChanges();
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

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.form.patchValue({ image: this.selectedFile.name });
    } else {
      this.selectedFile = null;
      this.form.patchValue({ image: null });
    }
    this.form.get('image')?.markAsTouched();
    this.form.get('image')?.updateValueAndValidity();
  }

  private loadProducts(): void {
    this.productsService.getSellerProducts().subscribe({
      next: (items) => {
        this.zone.run(() => {
          this.products = items.map(item => ({
            ...this.toSummary(item),
            tag: Number(item.tag)
          }));
          this.cdr.detectChanges();
        });
      }
    });
  }

  private appendProduct(raw: any): void {
    const summary = this.toSummary(raw);
    this.products = [summary, ...this.products.filter(p => p.id !== summary.id)];
    this.cdr.detectChanges();
  }

  private toSummary(raw: any): ProductSummary {
    const tag = this.toNumber(raw?.tag ?? raw?.Tag);
    const tagMeta = this.resolveTagMeta(tag);
    console.log(raw);
    return {
      id: this.toNumber(raw?.id ?? raw?.Id),
      title: this.toString(raw?.title ?? raw?.Title),
      description: this.toString(raw?.description ?? raw?.Description),
      price: this.toNumber(raw?.price ?? raw?.Price),
      category: this.toString(raw?.category ?? raw?.Category),
      image: this.resolveImage(raw?.image ?? raw?.Image),
      quantity: this.toNumber(raw?.quantity ?? raw?.Quantity),
      purchaseQuantity: this.toNumber(raw?.purchaseQuantity ?? raw?.PurchaseQuantity),
      tag: tag,
      tagLabel: tagMeta.label,
      tagSeverity: tagMeta.severity,
      dateOfSale: this.toIsoDate(raw?.dateOfSale ?? raw?.DateOfSale),
      sellerId: this.toNumber(raw?.sellerId ?? raw?.SellerId)
    };
  }

  trackByProductId(_: number, item: ProductSummary): number {
    return item.id;
  }

  private resolveTagMeta(value: number): { label: string; severity: TagSeverity } {
    const option: ProductTagOption | undefined = this.tagOptions.find(opt => opt.value === value);
    if (option) {
      return { label: option.label, severity: option.severity };
    }
    return { label: 'N/A', severity: 'info' };
  }

  private toNumber(value: any, fallback = 0): number {
    const numeric = typeof value === 'number' ? value : Number(value);
    return Number.isFinite(numeric) ? numeric : fallback;
  }

  private toString(value: any, fallback = ''): string {
    if (value == null) return fallback;
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
    return `${this.backendOrigin}/${str}`;
  }
}

