import { Component, OnInit } from '@angular/core';
import { MenubarModule } from 'primeng/menubar';
import { BadgeModule } from 'primeng/badge';
import { AvatarModule } from 'primeng/avatar';
import { InputTextModule } from 'primeng/inputtext';
import { CommonModule } from '@angular/common';
import { RippleModule } from 'primeng/ripple';
import { AuthService } from '../../services/auth/auth.service';
import { UsersService } from '../../services/users/users.service';

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

  constructor(private auth: AuthService, private users: UsersService) {}

  ngOnInit(): void {
    this.auth.currentUser$.subscribe((u) => {
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
    // Hydrate from access token on refresh
    if (this.auth.getAccessToken()) {
      this.users.getMe().subscribe({
        next: (res) => this.auth.setCurrentUserFromLoginResponse(res),
        error: () => this.updateMenu()
      });
    } else {
      this.updateMenu();
    }
  }

  updateMenu() {
    const isLoggedIn = !!this.auth.getAccessToken();
    this.items = [
      { label: 'Menu', icon: 'home', link: '' },
      { label: 'Mon Panier', icon: 'shopping_cart' },
      { label: 'Mon profil', icon: 'account_circle' },
      { label: 'Nous contacter', icon: 'mail' },
      ...(isLoggedIn
        ? [{ label: 'Déconnexion', icon: 'logout', command: () => this.onLogout() }]
        : [{ label: 'Connexion', icon: 'login', link: 'login' }])
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
