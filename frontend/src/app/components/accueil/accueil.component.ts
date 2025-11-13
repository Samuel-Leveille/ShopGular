import { AfterViewInit, Component, OnDestroy, NgZone, Renderer2 } from '@angular/core';
import { Router } from '@angular/router';
import { CarouselComponent } from '../carousel/carousel.component';

@Component({
  selector: 'app-accueil',
  standalone: true,
  imports: [CarouselComponent],
  templateUrl: './accueil.component.html',
  styleUrl: './accueil.component.css'
})
export class AccueilComponent implements AfterViewInit, OnDestroy {
  private searchInput?: HTMLInputElement | null;
  private searchButton?: HTMLButtonElement | null;
  private detachListeners: Array<() => void> = [];

  constructor(
    private readonly router: Router,
    private readonly zone: NgZone,
    private readonly renderer: Renderer2
  ) {}

  ngAfterViewInit(): void {
    this.searchInput = document.getElementById('searchBar') as HTMLInputElement | null;
    this.searchButton = document.getElementById('searchButton') as HTMLButtonElement | null;

    if (this.searchButton) {
      const disposeClick = this.renderer.listen(this.searchButton, 'click', () => this.triggerSearch());
      this.detachListeners.push(disposeClick);
    }

    if (this.searchInput) {
      const disposeKey = this.renderer.listen(this.searchInput, 'keydown', (event: KeyboardEvent) => {
        if (event.key === 'Enter') {
          event.preventDefault();
          this.triggerSearch();
        }
      });
      this.detachListeners.push(disposeKey);
    }
  }

  ngOnDestroy(): void {
    this.detachListeners.forEach(dispose => dispose());
    this.detachListeners = [];
  }

  private triggerSearch(): void {
    const term = this.searchInput?.value?.trim();
    if (!term) {
      return;
    }

    this.zone.run(() => {
      this.router.navigate(['/search'], {
        queryParams: { q: term, page: 1 }
      });
    });
  }
}
