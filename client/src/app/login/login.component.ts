import { AccountService } from '../_services/account.service';
import { LoginModel } from '../_models/login.model';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  credentials: LoginModel = {
    username: '',
    password: '',
  };

  registerMode = false;

  constructor(
    public accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    if (this.accountService.currentUser$) this.router.navigateByUrl('/home');
  }

  login() {
    if (this.credentials)
      this.accountService.login(this.credentials).subscribe({
        next: (_) => {
          this.router.navigateByUrl('/home');
        },

        error: (error) => {
          this.toastr.error(error.error), console.log(error);
        },
      });
    return;
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  cancelRegisterMode(event: boolean) {
    this.registerMode = event;
  }
}
