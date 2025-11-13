import { ProductSummary } from './product-summary';

export interface ProductDetail extends ProductSummary {
  sellerId: number;
  sellerName: string;
  sellerEmail: string;
}

