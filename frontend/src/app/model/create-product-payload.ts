export interface CreateProductPayload {
  title: string;
  description: string;
  price: number;
  category: string;
  image: string;
  quantity: number;
  purchaseQuantity?: number;
  tag?: number;
  dateOfSale?: string;
}

