import { Routes } from '@angular/router';
import { SignupComponent } from './components/signup/signup.component';
import { AccueilComponent } from './components/accueil/accueil.component';
import { LoginComponent } from './components/login/login.component';
import { GoogleCallbackComponent } from './components/google-callback/google-callback.component';
import { NewProductComponent } from './components/new-product/new-product.component';
import { SearchResultsComponent } from './components/search-results/search-results.component';
import { ProductDetailComponent } from './components/product-detail/product-detail.component';
import { CartComponent } from './components/cart/cart.component';

export const routes: Routes = [
    { path: '', component: AccueilComponent },
    { path: 'signup', component: SignupComponent },
    { path: 'login', component: LoginComponent },
    { path: 'search', component: SearchResultsComponent },
    { path: 'product/:id', component: ProductDetailComponent },
    { path: 'cart', component: CartComponent },
    { path: 'seller/new-product', component: NewProductComponent },
    { path: 'google-callback', component: GoogleCallbackComponent }
];
