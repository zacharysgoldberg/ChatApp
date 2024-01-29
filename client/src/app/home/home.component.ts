import { HttpClient } from '@angular/common/http';
import { AccountService } from '../_services/account.service';
import { LoginModel } from '../_models/login.model';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  registerMode = false;

  users: any;
  credentials: LoginModel = { username: '', password: '' };

  constructor(
    private http: HttpClient,
    public accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.getUsers();
  }

  login() {
    this.accountService.login(this.credentials).subscribe({
      next: (_) => {
        this.router.navigateByUrl('/account');
      },

      error: (error) => {
        this.toastr.error(error.error), console.log(error);
      },
    });
  }

  getUsers() {
    const baseUrl: string = this.accountService.baseUrl;
    this.http.get(baseUrl + 'users').subscribe({
      next: (response) => (this.users = response),
      error: (error) => console.log(error),
      complete: () => console.log('Request has completed'),
    });
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  cancelRegisterMode(event: boolean) {
    this.registerMode = event;
  }
}
