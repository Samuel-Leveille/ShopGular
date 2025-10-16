import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SellerSignup } from '../../model/sellerSignup';
import { UserLogin } from '../../model/userLogin';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private http: HttpClient) { }

  api: string = "https://localhost:7103/api/user/";

  signUpSeller(seller: SellerSignup) {
    return this.http.post<SellerSignup>(this.api + "seller/signup", seller);
  }

  login(user: UserLogin) {
    return this.http.post<UserLogin>(this.api + "login", user);
  }
}
