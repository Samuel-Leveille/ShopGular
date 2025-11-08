import { Routes } from '@angular/router';
import { SignupComponent } from './components/signup/signup.component';
import { AccueilComponent } from './components/accueil/accueil.component';
import { LoginComponent } from './components/login/login.component';
import { GoogleCallbackComponent } from './components/google-callback/google-callback.component';

export const routes: Routes = [
    {path: '', component: AccueilComponent},
    {path: 'signup', component: SignupComponent},
    {path: 'login', component: LoginComponent},
    {path: 'google-callback', component: GoogleCallbackComponent}
];
