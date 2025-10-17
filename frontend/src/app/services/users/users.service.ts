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
    return this.http.post<SellerSignup>(this.api + "seller/signup", seller);
  }

  signUpClient(client: ClientSignup) {
    return this.http.post<ClientSignup>(this.api + "client/signup", client);
  }

  login(user: UserLogin) {
    return this.http.post<UserLogin>(this.api + "login", user);
  }
}
