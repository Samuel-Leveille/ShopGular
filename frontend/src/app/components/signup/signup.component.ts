import { Component } from '@angular/core';
import { SellerSignup } from '../../model/sellerSignup';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../services/users/users.service';
import { UserLogin } from '../../model/userLogin';
import { ClientSignup } from '../../model/clientSignup';
import { NgIf } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [FormsModule, NgIf, RouterLink],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {

  constructor(private _usersService: UsersService, private _router: Router, private _auth: AuthService) {}

  lienConnexion: string = "login";

  sellerSignup: SellerSignup = new SellerSignup;
  clientSignup: ClientSignup = new ClientSignup;
  user: any;

  isSeller: boolean = false;

  onSignup() {
    if (this.isSeller) {
      this.user = this._usersService.signUpSeller(this.sellerSignup).subscribe((newUser => {
        if (this.sellerSignup.email === newUser.email) {
          this.login(this.sellerSignup.email, this.sellerSignup.password);
        }
      }));
    } else {
      this.user = this._usersService.signUpClient(this.clientSignup).subscribe((newUser => {
        if (this.clientSignup.email === newUser.email) {
          this.login(this.clientSignup.email, this.clientSignup.password);
        }
      }));
    }
  }

  login(email: string, password: string) {
    let userLogin: UserLogin = new UserLogin(email, password);
    this._usersService.login(userLogin).subscribe((res: any) => {
      if (res?.accessToken && res?.refreshToken) {
        this._auth.saveTokens(res.accessToken, res.refreshToken);
        this._auth.setCurrentUserFromLoginResponse(res);
      }
      this._router.navigate(['/']);
    });
  }

  onIsSellerChanged(value: boolean) {
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
