import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { UserModel } from '../_models/user.model';
import { take } from 'rxjs';
import { MemberModel } from '../_models/member.model';
import { UserService } from '../_services/user.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  user?: UserModel;
  member: MemberModel | undefined;

  constructor(
    public accountService: AccountService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.user = user;
        }
      },
    });

    const username = this.accountService.getUsername();
    if (!username) return;

    this.userService.getMember(username).subscribe({
      next: (member) => {
        this.member = member;
      },
    });
  }
}
