import { Component, OnInit } from '@angular/core';
import { MenubarModule } from 'primeng/menubar';
import { BadgeModule } from 'primeng/badge';
import { AvatarModule } from 'primeng/avatar';
import { InputTextModule } from 'primeng/inputtext';
import { CommonModule } from '@angular/common';
import { RippleModule } from 'primeng/ripple';
import { AuthService } from '../../services/auth/auth.service';
import { UsersService } from '../../services/users/users.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [MenubarModule, BadgeModule, AvatarModule, InputTextModule, RippleModule, CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {

  items: any[] | undefined;
  userName: string | null = null;
  firstName: string | null = null;
  private userType: string | null = null;

  constructor(private auth: AuthService, private users: UsersService) {}

  ngOnInit(): void {
    this.auth.currentUser$.subscribe((u) => {
      this.userType = u?.type ?? null;
      if (u?.type === 'client') {
        this.firstName = u.user?.FirstName || u.user?.firstName || u.user?.firstname || null;
        this.userName = u.user?.Name || u.user?.name || null;
      } else if (u?.type === 'seller') {
        this.firstName = null;
        this.userName = u.user?.Name || u.user?.name || null;
      } else {
        this.firstName = null;
        this.userName = u?.user?.Name || u?.user?.name || null;
      }
      this.updateMenu();
    });

    if (this.auth.getAccessToken()) {
      this.users.getMe().subscribe({
        next: (res) => this.auth.setCurrentUserFromLoginResponse(res),
        error: (err: HttpErrorResponse) => {
          if (err.status === 401) {
            this.auth.clearTokens();
          }
          this.updateMenu();
        }
      });
    } else {
      this.updateMenu();
    }
  }

  updateMenu() {
    const isLoggedIn = !!this.auth.getAccessToken();
    const current = this.auth.getCurrentUserSnapshot();
    const isSeller = current?.type === 'seller';

    this.items = [
      { label: 'Menu', icon: 'home', routerLink: '/' },
      isSeller
        ? { label: 'Nouveau produit', icon: 'add_circle', routerLink: '/seller/new-product' }
        : { label: 'Mon panier', icon: 'shopping_cart', routerLink: '/' },
      { label: 'Mon profil', icon: 'account_circle', routerLink: '/' },
      { label: 'Nous contacter', icon: 'mail', routerLink: '/' },
      ...(isLoggedIn
        ? [{ label: 'Déconnexion', icon: 'logout', command: () => this.onLogout() }]
        : [{ label: 'Connexion', icon: 'login', routerLink: '/login' }])
    ];
  }

  onLogout() {
    const rt = this.auth.getRefreshToken();
    this.users.logout(rt).subscribe({
      next: () => {
        this.auth.clearTokens();
        this.userName = null;
        this.firstName = null;
        location.href = '/';
      },
      error: () => {
        this.auth.clearTokens();
        this.userName = null;
        this.firstName = null;
        location.href = '/';
      }
    });
  }
}
