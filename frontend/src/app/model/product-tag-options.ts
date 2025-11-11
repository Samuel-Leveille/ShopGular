export type TagSeverity = 'success' | 'warning' | 'danger' | 'info';

export interface ProductTagOption {
  label: string;
  value: number;
  severity: TagSeverity;
}

export const PRODUCT_TAG_OPTIONS: ProductTagOption[] = [
  { label: 'En stock', value: 0, severity: 'success' },
  { label: 'Stock faible', value: 1, severity: 'warning' },
  { label: 'Rupture', value: 2, severity: 'danger' }
];

