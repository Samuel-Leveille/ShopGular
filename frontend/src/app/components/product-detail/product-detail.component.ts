import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { AuthService } from '../../services/auth/auth.service';
import { ProductsService } from '../../services/products/products.service';
import { ProductDetail } from '../../model/product-detail';
import { CartService } from '../../services/cart/cart.service';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    ButtonModule,
    TagModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css'
})
export class ProductDetailComponent implements OnInit, OnDestroy {
  product: ProductDetail | null = null;
  loading = false;
  updating = false;
  editing = false;
  isClient = false;
  isOwner = false;

  form: FormGroup;
  private selectedFile: File | null = null;
  private subscriptions: Subscription[] = [];

  constructor(
    private readonly route: ActivatedRoute,
    private readonly productsService: ProductsService,
    private readonly cartService: CartService,
    private readonly auth: AuthService,
    private readonly fb: FormBuilder,
    private readonly cdr: ChangeDetectorRef,
    private readonly messageService: MessageService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(120)]],
      description: ['', [Validators.required, Validators.maxLength(2000)]],
      quantity: [0, [Validators.required, Validators.min(0)]],
      image: ['']
    });
  }

  ngOnInit(): void {
    const sub = this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      const id = idParam ? Number(idParam) : NaN;
      if (!Number.isFinite(id) || id <= 0) {
        this.messageService.add({ severity: 'warn', summary: 'Produit introuvable', detail: 'Identifiant de produit invalide.' });
        return;
      }
      this.loadProduct(id);
    });
    this.subscriptions.push(sub);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  beginEdit(): void {
    if (!this.product) {
      return;
    }
    this.editing = true;
    this.selectedFile = null;
    this.form.reset({
      title: this.product.title,
      description: this.product.description,
      quantity: this.product.quantity,
      image: ''
    });
    this.cdr.markForCheck();
  }

  cancelEdit(): void {
    this.editing = false;
    this.selectedFile = null;
    this.form.reset();
    this.cdr.markForCheck();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.form.patchValue({ image: this.selectedFile.name });
    } else {
      this.selectedFile = null;
      this.form.patchValue({ image: '' });
    }
  }

  submitEdit(): void {
    if (!this.product || !this.editing) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.messageService.add({ severity: 'warn', summary: 'Formulaire incomplet', detail: 'Veuillez corriger les champs requis.' });
      return;
    }

    const value = this.form.value;
    const quantity = Number(value.quantity);
    if (Number.isNaN(quantity) || quantity < 0) {
      this.messageService.add({ severity: 'error', summary: 'Quantité invalide', detail: 'La quantité doit être un entier positif.' });
      return;
    }

    const formData = new FormData();
    formData.append('Title', value.title.trim());
    formData.append('Description', value.description.trim());
    formData.append('Quantity', quantity.toString());
    if (this.selectedFile) {
      formData.append('ImageFile', this.selectedFile, this.selectedFile.name);
    }

    this.updating = true;
    this.productsService.updateSellerProduct(this.product.id, formData).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Produit mis à jour', detail: 'Les informations ont été mises à jour.' });
        this.cancelEdit();
        this.reloadProduct();
      },
      error: (err) => {
        const message = err?.error?.message ?? 'Impossible de mettre à jour le produit.';
        this.messageService.add({ severity: 'error', summary: 'Erreur', detail: message });
      },
      complete: () => {
        this.updating = false;
        this.cdr.markForCheck();
      }
    });
  }

  addToCart(): void {
    if (!this.product) {
      return;
    }
    if (!this.isClient) {
      this.messageService.add({ severity: 'warn', summary: 'Réservé aux clients', detail: 'Connectez-vous en tant que client pour ajouter au panier.' });
      return;
    }
    if (this.product.quantity <= 0) {
      this.messageService.add({ severity: 'warn', summary: 'Indisponible', detail: 'Ce produit est actuellement en rupture de stock.' });
      return;
    }
    this.cartService.addProduct(this.product, 1);
    this.messageService.add({ severity: 'success', summary: 'Ajouté au panier', detail: `${this.product.title} a été ajouté à votre panier.` });
  }

  private loadProduct(id: number): void {
    this.loading = true;
    this.cdr.markForCheck();
    const sub = this.productsService.getProductDetail(id).subscribe({
      next: (detail) => {
        this.product = detail;
        this.updatePermissions();
      },
      error: () => {
        this.product = null;
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Produit introuvable', detail: 'Ce produit n’existe plus.' });
        this.cdr.markForCheck();
      },
      complete: () => {
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
    this.subscriptions.push(sub);
  }

  private reloadProduct(): void {
    if (!this.product) {
      return;
    }
    this.loadProduct(this.product.id);
  }

  private updatePermissions(): void {
    const current = this.auth.getCurrentUserSnapshot();
    const type = current?.type;
    const user = current?.user;
    const userId = typeof user?.Id === 'number' ? user.Id : Number(user?.id ?? user?.Id);

    this.isClient = type === 'client';
    this.isOwner = type === 'seller' && this.product?.sellerId === userId;
    this.cdr.markForCheck();
  }
}

