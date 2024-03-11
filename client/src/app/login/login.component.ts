import { AccountService } from '../_services/account.service';
import { LoginModel } from '../_models/login.model';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Component, OnInit } from '@angular/core';
import { UserModel } from '../_models/user.model';
import { first, take } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  user?: UserModel;
  registerMode = false;
  forgotPasswordMode = false;

  credentials: LoginModel = {
    username: '',
    password: '',
  };

  constructor(
    public accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: async (user) => {
        if (user) {
          this.user = await this.accountService.getAuthenticatedUser(user);

          if (this.user) {
            this.router.navigateByUrl('/home');
          } else this.accountService.logout();
        }
      },
    });
  }

  login() {
    if (this.credentials)
      this.accountService.login(this.credentials).subscribe({
        next: (_) => {
          this.router.navigateByUrl('/home');
        },

        error: (error) => {
          console.log(error);
        },
      });
    return;
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  forgotPasswordToggle() {
    this.forgotPasswordMode = !this.forgotPasswordMode;
  }
}
