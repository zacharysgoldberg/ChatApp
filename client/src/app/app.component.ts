import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';
import { UserModel } from './_models/user.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  public title: string = 'Chat App';

  constructor(private accountService: AccountService) {}

  ngOnInit(): void {
    this.setCurrentUser();
  }

  setCurrentUser() {
    const userString = JSON.stringify(localStorage.getItem('username'));
    if (!userString) return;
    const user: UserModel = JSON.parse(userString);
    this.accountService.setCurrentUserSource(user);
  }
}
