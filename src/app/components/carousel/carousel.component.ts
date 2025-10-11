import { Component, OnInit } from '@angular/core';
import { CarouselModule } from 'primeng/carousel';
import { ProductsService } from '../../services/products/products.service';
import { Product } from '../../model/product';

@Component({
  selector: 'app-carousel',
  standalone: true,
  imports: [CarouselModule],
  templateUrl: './carousel.component.html',
  styleUrl: './carousel.component.css'
})
export class CarouselComponent implements OnInit {

    constructor(private productService: ProductsService) {}

    products: Product[] = [];

    responsiveOptions: any[] | undefined;
  
    ngOnInit(): void {
      this.productService.getProducts().subscribe(products => {
        this.products = products;
      });

      this.responsiveOptions = [
        {
            breakpoint: '1199px',
            numVisible: 1,
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
