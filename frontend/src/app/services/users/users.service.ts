import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SellerSignup } from '../../model/sellerSignup';
import { UserLogin } from '../../model/userLogin';
import { ClientSignup } from '../../model/clientSignup';
import { GoogleSignupPayload } from '../../model/google-signup-payload';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private http: HttpClient) { }

  private readonly api = environment.apiBaseUrl;

  signUpSeller(seller: SellerSignup) {
    return this.http.post<any>(this.api + 'seller/signup', seller);
  }

  signUpClient(client: ClientSignup) {
    return this.http.post<any>(this.api + 'client/signup', client);
  }

  login(user: UserLogin) {
    return this.http.post<any>(this.api + 'login', user);
  }

  completeGoogleSignup(payload: GoogleSignupPayload) {
    return this.http.post<any>(this.api + 'google/complete-signup', payload);
  }

  startGoogleOAuth(returnUrl: string) {
    return this.http.get<{ authorizationUrl: string }>(this.api + 'google/oauth/start', {
      params: { returnUrl }
    });
  }

  getGoogleOAuthResult(state: string) {
    return this.http.get<any>(this.api + `google/oauth/result/${state}`);
  }

  logout(refreshToken: string | null) {
    return this.http.post<void>(this.api + 'logout', { refreshToken });
  }

  getMe() {
    return this.http.get<any>(this.api + 'me');
  }
}
