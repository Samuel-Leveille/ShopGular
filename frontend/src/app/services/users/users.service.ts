import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SellerSignup } from '../../model/sellerSignup';
import { UserLogin } from '../../model/userLogin';
import { ClientSignup } from '../../model/clientSignup';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private http: HttpClient) { }

  api: string = "http://localhost:5227/api/user/";

  signUpSeller(seller: SellerSignup) {
    // Backend retourne SellerDto
    return this.http.post<any>(this.api + "seller/signup", seller);
  }

  signUpClient(client: ClientSignup) {
    // Backend retourne ClientDto
    return this.http.post<any>(this.api + "client/signup", client);
  }

  login(user: UserLogin) {
    // Backend retourne { user, accessToken, refreshToken }
    return this.http.post<any>(this.api + "login", user);
  }

  logout(refreshToken: string | null) {
    return this.http.post<void>(this.api + "logout", { refreshToken });
  }

  getMe() {
    return this.http.get<any>(this.api + "me");
  }
}
