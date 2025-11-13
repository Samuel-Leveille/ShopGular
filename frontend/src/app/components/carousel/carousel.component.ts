import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CarouselModule } from 'primeng/carousel';
import { ProductsService } from '../../services/products/products.service';
import { Product } from '../../model/productInterface';
import { CategoryProducts } from '../../model/category-products';

@Component({
  selector: 'app-carousel',
  standalone: true,
  imports: [CommonModule, CarouselModule],
  templateUrl: './carousel.component.html',
  styleUrl: './carousel.component.css'
})
export class CarouselComponent implements OnInit {

    constructor(private productService: ProductsService, private readonly router: Router) {}

    products: Product[] = [];
    categoryBlocks: CategoryProducts[] = [];
    loadingCategories = false;

    responsiveOptions: any[] = [];
    categoryResponsive: any[] = [];
  
    ngOnInit(): void {
      this.initResponsiveOptions();
      this.productService.getProducts().subscribe(products => {
        this.products = products;
      });
      this.fetchCategoryCarousels();
    }

    trackCategory(index: number, block: CategoryProducts): string {
      return block.category ?? `cat-${index}`;
    }

    openProduct(productId: number): void {
      this.router.navigate(['/product', productId]);
    }

    private fetchCategoryCarousels(): void {
      this.loadingCategories = true;
      this.productService.getProductsByCategory().subscribe({
        next: (blocks) => {
          this.categoryBlocks = blocks;
        },
        error: (err) => {
          console.error('Erreur lors du chargement des produits par catégorie', err);
          this.categoryBlocks = [];
        },
        complete: () => {
          this.loadingCategories = false;
        }
      });
    }

    private initResponsiveOptions(): void {
      this.responsiveOptions = [
        {
          breakpoint: '1199px',
          numVisible: 2,
          numScroll: 1
        },
        {
          breakpoint: '991px',
          numVisible: 2,
          numScroll: 1
        },
        {
          breakpoint: '767px',
          numVisible: 1,
          numScroll: 1
        }
      ];

      this.categoryResponsive = [
        {
          breakpoint: '1199px',
          numVisible: 2,
          numScroll: 1
        },
        {
          breakpoint: '991px',
          numVisible: 2,
          numScroll: 1
        },
        {
          breakpoint: '767px',
          numVisible: 1,
          numScroll: 1
        }
      ];
    }
}
