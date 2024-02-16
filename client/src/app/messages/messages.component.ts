import { Component, OnInit } from '@angular/core';
import { MessageModel } from '../_models/message.model';
import { Pagination } from '../_models/pagination.mode';
import { MessageService } from '../_services/message.service';
import { AccountService } from '../_services/account.service';
import { UserModel } from '../_models/user.model';
import { take } from 'rxjs';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
})
export class MessagesComponent implements OnInit {
  messages?: MessageModel[];
  pagination?: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  user: UserModel | undefined;

  constructor(
    public accountService: AccountService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });

    this.loadMessages();
  }

  async loadMessages() {
    if (!this.user) {
      return;
    }

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.messageService
      .getMessages(this.pageNumber, this.pageSize, this.container)
      .subscribe({
        next: (response) => {
          this.messages = response.result;
          this.pagination = response.pagination;
        },
      });
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
