import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UsersService } from '../../services/users/users.service';
import { UserLogin } from '../../model/userLogin';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  lienInscription: string = "signup";

  constructor(private _usersService: UsersService, private _router: Router, private _auth: AuthService) {}

  credentials: UserLogin = new UserLogin('', '');

  onSubmit() {
    this._usersService.login(this.credentials).subscribe((res: any) => {
      if (res?.accessToken && res?.refreshToken) {
        this._auth.saveTokens(res.accessToken, res.refreshToken);
        this._auth.setCurrentUserFromLoginResponse(res);
      }
      this._router.navigate(['/']);
    });
  }
}
