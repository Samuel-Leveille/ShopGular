import { Component, OnInit } from '@angular/core';
import { SellerSignup } from '../../model/sellerSignup';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../services/users/users.service';
import { UserLogin } from '../../model/userLogin';
import { ClientSignup } from '../../model/clientSignup';
import { NgIf } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { GoogleSignupPayload } from '../../model/google-signup-payload';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [FormsModule, NgIf, RouterLink],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent implements OnInit {

  constructor(
    private _usersService: UsersService,
    private _router: Router,
    private _auth: AuthService,
    private _route: ActivatedRoute
  ) {}

  lienConnexion: string = "login";

  sellerSignup: SellerSignup = new SellerSignup;
  clientSignup: ClientSignup = new ClientSignup;
  user: any;

  isSeller: boolean = false;
  isGoogleSignup: boolean = false;
  private returnUrl: string | null = null;
  errorMessage: string | null = null;
  isSubmitting = false;

  ngOnInit(): void {
    this._route.queryParamMap.subscribe(params => {
      this.returnUrl = params.get('returnUrl');
      this.isGoogleSignup = params.get('google') === '1';
      const accountType = params.get('accountType');
      if (accountType === 'seller') {
        this.isSeller = true;
      }

      if (this.isGoogleSignup) {
        const pendingToken = this._auth.peekGooglePendingToken();
        if (!pendingToken) {
          this.errorMessage = 'La session Google a expiré. Veuillez recommencer la connexion.';
        }

        const email = params.get('email') ?? '';
        const firstName = params.get('firstName') ?? '';
        const lastName = params.get('lastName') ?? '';
        this.clientSignup.email = email;
        this.clientSignup.firstname = firstName || this.clientSignup.firstname;
        this.clientSignup.name = lastName || this.clientSignup.name;
        this.sellerSignup.email = email;
      }
    });
  }

  onSignup() {
    this.errorMessage = null;

    if (this.isGoogleSignup) {
      const pendingToken = this._auth.peekGooglePendingToken();
      if (!pendingToken) {
        this.errorMessage = 'La session Google a expiré. Veuillez recommencer.';
        return;
      }

      const payload: GoogleSignupPayload = {
        pendingToken,
        accountType: this.isSeller ? 'seller' : 'client'
      };

      if (this.isSeller) {
        if (!this.sellerSignup.name || !this.sellerSignup.name.trim()) {
          this.errorMessage = 'Veuillez indiquer le nom de votre entreprise.';
          return;
        }
        payload.sellerName = this.sellerSignup.name.trim();
      } else {
        payload.firstName = this.clientSignup.firstname?.trim();
        payload.lastName = this.clientSignup.name?.trim();
      }

      this.isSubmitting = true;
      this._usersService.completeGoogleSignup(payload).subscribe({
        next: (res: any) => {
          this._auth.consumeGooglePendingToken();
          if (res?.loginPayload?.accessToken && res?.loginPayload?.refreshToken) {
            this._auth.saveTokens(res.loginPayload.accessToken, res.loginPayload.refreshToken);
            this._auth.setCurrentUserFromLoginResponse(res.loginPayload);
          } else if (res?.accessToken && res?.refreshToken) {
            this._auth.saveTokens(res.accessToken, res.refreshToken);
            this._auth.setCurrentUserFromLoginResponse(res);
          }
          this._router.navigate([this.returnUrl || '/']);
        },
        error: (err) => {
          console.error('Erreur lors de la finalisation Google', err);
          this.errorMessage = err?.error?.message ?? 'Impossible de finaliser votre inscription Google.';
        },
        complete: () => {
          this.isSubmitting = false;
        }
      });
      return;
    }

    if (this.isSeller) {
      this.isSubmitting = true;
      this.user = this._usersService.signUpSeller(this.sellerSignup).subscribe({
        next: (newUser => {
          if (this.sellerSignup.email === newUser.email) {
            this.login(this.sellerSignup.email, this.sellerSignup.password);
          }
        }),
        error: (err) => {
          console.error('Erreur lors de l’inscription vendeur', err);
          this.errorMessage = err?.error?.message ?? 'Impossible de créer le compte vendeur.';
          this.isSubmitting = false;
        }
      });
    } else {
      this.isSubmitting = true;
      this.user = this._usersService.signUpClient(this.clientSignup).subscribe({
        next: (newUser => {
          if (this.clientSignup.email === newUser.email) {
            this.login(this.clientSignup.email, this.clientSignup.password);
          }
        }),
        error: (err) => {
          console.error('Erreur lors de l’inscription client', err);
          this.errorMessage = err?.error?.message ?? 'Impossible de créer le compte client.';
          this.isSubmitting = false;
        }
      });
    }
  }

  login(email: string, password: string) {
    let userLogin: UserLogin = new UserLogin(email, password);
    this._usersService.login(userLogin).subscribe({
      next: (res: any) => {
        if (res?.accessToken && res?.refreshToken) {
          this._auth.saveTokens(res.accessToken, res.refreshToken);
          this._auth.setCurrentUserFromLoginResponse(res);
        }
        this._router.navigate([this.returnUrl || '/']);
      },
      error: (err) => {
        console.error('Erreur lors de la connexion post-inscription', err);
        this.errorMessage = err?.error?.message ?? 'La connexion automatique a échoué. Veuillez vous connecter manuellement.';
        this.isSubmitting = false;
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }

  onIsSellerChanged(value: boolean) {
    this.isSeller = value;
    if (this.isGoogleSignup) {
      if (value) {
        this.sellerSignup.email = this.clientSignup.email || this.sellerSignup.email;
      } else {
        this.clientSignup.email = this.sellerSignup.email || this.clientSignup.email;
      }
      return;
    }

    if (value) {
      this.clientSignup.email = "";
      this.clientSignup.firstname = "";
      this.clientSignup.name = "";
      this.clientSignup.password = "";
      this.clientSignup.confirmPassword = "";
    } else {
      this.sellerSignup.email = "";
      this.sellerSignup.name = "";
      this.sellerSignup.password = "";
      this.sellerSignup.confirmPassword = "";
    }
  }

}
