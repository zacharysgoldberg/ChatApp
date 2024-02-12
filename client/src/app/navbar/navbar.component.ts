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
import { MemberService } from '../_services/member.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent implements OnInit {
  member: MemberModel | undefined;
  notifications: NotificationModel[] = [];
  isDropup = true;

  constructor(
    public accountService: AccountService,
    private memberService: MemberService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    const username = this.accountService.getUsername();

    if (!username) return;

    this.memberService.getMember(username).subscribe({
      next: (member) => {
        this.member = member;
      },
    });
  }

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
    this.router.navigateByUrl('/login');
  }

  navigateToProfile() {
    this.router.navigateByUrl('/profile');
  }

  loadNotifications() {}
}
