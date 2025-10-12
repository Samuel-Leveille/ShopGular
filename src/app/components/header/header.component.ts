import { Component, OnInit } from '@angular/core';
import { MenubarModule } from 'primeng/menubar';
import { BadgeModule } from 'primeng/badge';
import { AvatarModule } from 'primeng/avatar';
import { InputTextModule } from 'primeng/inputtext';
import { CommonModule } from '@angular/common';
import { RippleModule } from 'primeng/ripple';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [MenubarModule, BadgeModule, AvatarModule, InputTextModule, RippleModule, CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {

  items: any[] | undefined;

  ngOnInit(): void {
    this.items = [
      {
          label: 'Menu',
          icon: 'home',
          link: ''
      },
      {
          label: 'Mon Panier',
          icon: 'shopping_cart',
      },
      {
          label: 'Mon profil',
          icon: 'account_circle'
      },
      {
          label: 'Nous contacter',
          icon: 'mail'
      },
      {
          label: 'Connexion',
          icon: 'login',
          link: 'login'
      }
  ];
  }
}
