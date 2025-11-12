import { TagSeverity } from './product-tag-options';

export interface ProductSummary {
  id: number;
  title: string;
  description: string;
  price: number;
  category: string;
  image: string;
  quantity: number;
  purchaseQuantity: number;
  tag: number;
  tagLabel: string;
  tagSeverity: TagSeverity;
  dateOfSale: string;
  sellerId: number;
}

