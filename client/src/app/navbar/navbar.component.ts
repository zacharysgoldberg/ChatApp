import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Component, Output, OnInit } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent implements OnInit {
  isDropup = true;

  model: any = {};

  constructor(
    public accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {}

  logout() {
    // this.accountService.logout(this.model).subscribe({
    //   next: () => {
    //     // console.log(response);
    //     this.router.navigateByUrl('/');
    //   },

    //   error: (error) => {
    //     this.toastr.error(error.error), console.log(error);
    //   },
    // });

    this.accountService.logout(this.model);
    this.router.navigateByUrl('/');
  }

  navigateToProfile(): void {
    // Implement navigation logic to the user's profile page
    this.router.navigate(['/profile']);
  }
}
