import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Component, Output, OnInit } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { NotificationModel } from '../_models/notification.model';
import { UserModel } from '../_models/user.model';
import { take } from 'rxjs';
import { MemberModel } from '../_models/member.model';
import { ContactsService } from '../_services/contacts.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent implements OnInit {
  isDropup = true;
  model: any = {};
  notifications: NotificationModel[] = [];

  constructor(
    public accountService: AccountService,
    private contactService: ContactsService,
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

    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  navigateToProfile() {
    this.router.navigateByUrl('/profile');
  }

  loadNotifications() {}
}
