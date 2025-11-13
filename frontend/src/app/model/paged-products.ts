import { ProductSummary } from './product-summary';

export interface PagedProducts {
  items: ProductSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

