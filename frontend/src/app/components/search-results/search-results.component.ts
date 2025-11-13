import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { ProductsService } from '../../services/products/products.service';
import { ProductSummary } from '../../model/product-summary';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.css'
})
export class SearchResultsComponent implements OnInit, OnDestroy {
  query = '';
  loading = false;
  page = 1;
  readonly pageSize = 20;
  totalCount = 0;
  results: ProductSummary[] = [];

  private paramsSubscription?: Subscription;
  private searchSubscription?: Subscription;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly productsService: ProductsService
  ) {}

  ngOnInit(): void {
    this.paramsSubscription = this.route.queryParamMap.subscribe(params => {
      const q = params.get('q') ?? '';
      const pageParam = Number(params.get('page')) || 1;
      const normalizedQuery = q.trim();

      const queryChanged = normalizedQuery !== this.query;
      const pageChanged = pageParam !== this.page;

      if (queryChanged || pageChanged) {
        this.query = normalizedQuery;
        this.page = Math.max(pageParam, 1);
        this.fetchResults();
      }
    });
  }

  ngOnDestroy(): void {
    this.paramsSubscription?.unsubscribe();
    this.searchSubscription?.unsubscribe();
  }

  get totalPages(): number {
    return Math.max(Math.ceil(this.totalCount / this.pageSize), 1);
  }

  onPrevious(): void {
    this.navigateToPage(this.page - 1);
  }

  onNext(): void {
    this.navigateToPage(this.page + 1);
  }

  viewProduct(product: ProductSummary): void {
    this.router.navigate(['/product', product.id]);
  }

  private navigateToPage(target: number): void {
    if (target < 1 || target > this.totalPages) {
      return;
    }

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: this.query, page: target },
      queryParamsHandling: 'merge'
    });
  }

  private fetchResults(): void {
    this.searchSubscription?.unsubscribe();

    if (!this.query) {
      this.results = [];
      this.totalCount = 0;
      this.loading = false;
      return;
    }

    this.loading = true;
    this.searchSubscription = this.productsService
      .searchProducts(this.query, this.page, this.pageSize)
      .subscribe({
        next: res => {
          this.results = res.items;
          this.totalCount = res.totalCount;
          this.page = res.page;
        },
        error: () => {
          this.results = [];
          this.totalCount = 0;
        },
        complete: () => {
          this.loading = false;
        }
      });
  }
}

