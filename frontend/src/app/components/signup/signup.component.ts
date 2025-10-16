import { Component } from '@angular/core';
import { SellerSignup } from '../../model/sellerSignup';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../services/users/users.service';
import { UserLogin } from '../../model/userLogin';
import { ClientSignup } from '../../model/clientSignup';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [FormsModule, NgIf],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {

  constructor(private _usersService: UsersService) {}

  lienConnexion: string = "login";

  sellerSignup: SellerSignup = new SellerSignup;
  clientSignup: ClientSignup = new ClientSignup;
  user: any;

  isSeller: boolean = false;

  onSignupSeller() {
    this.user = this._usersService.signUpSeller(this.isSeller ? this.sellerSignup : this.clientSignup).subscribe((newUser => {
      if (this.sellerSignup.email === newUser.email) {
        this.login(newUser);
      }
    }))
  }

  login(user: any) {
    let userLogin: UserLogin = new UserLogin(user.email, user.password);
    this._usersService.login(userLogin);
  }
}
