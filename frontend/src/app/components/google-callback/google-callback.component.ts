import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { UsersService } from '../../services/users/users.service';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './google-callback.component.html'
})
export class GoogleCallbackComponent implements OnInit {
  status: 'loading' | 'error' = 'loading';
  message = 'Connexion en cours...';

  constructor(
    private auth: AuthService,
    private usersService: UsersService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const error = this.route.snapshot.queryParamMap.get('error');
    if (error) {
      this.status = 'error';
      const message = this.route.snapshot.queryParamMap.get('message');
      this.message = message || "La connexion via Google a été annulée.";
      return;
    }

    const state = this.route.snapshot.queryParamMap.get('state');
    if (!state) {
      this.status = 'error';
      this.message = 'Paramètre de session Google manquant.';
      return;
    }

    this.usersService.getGoogleOAuthResult(state).subscribe({
      next: (res: any) => {
        const returnUrl = res?.returnUrl || '/';
        const result = res?.result;
        if (!result) {
          this.status = 'error';
          this.message = 'Réponse Google invalide.';
          return;
        }
        if (result?.requiresCompletion) {
          if (!result?.pendingToken) {
            this.status = 'error';
            this.message = 'Les informations Google de création de compte sont incomplètes.';
            return;
          }
          this.auth.setGooglePendingToken(result.pendingToken);
          const profile = result.profile || {};
          this.router.navigate(['/signup'], {
            queryParams: {
              google: '1',
              email: profile.email || '',
              firstName: profile.firstName || profile.givenName || '',
              lastName: profile.lastName || profile.familyName || '',
              fullName: profile.fullName || profile.name || '',
              returnUrl
            }
          });
          return;
        }

        const loginPayload = result.loginPayload;
        if (loginPayload?.accessToken && loginPayload?.refreshToken) {
          this.auth.saveTokens(loginPayload.accessToken, loginPayload.refreshToken);
          this.auth.setCurrentUserFromLoginResponse(loginPayload);
        }
        this.router.navigateByUrl(returnUrl || '/');
      },
      error: (err) => {
        console.error('Erreur lors de la connexion Google', err);
        this.status = 'error';
        this.message = 'La connexion Google a échoué. Veuillez réessayer.';
      }
    });
  }
}
