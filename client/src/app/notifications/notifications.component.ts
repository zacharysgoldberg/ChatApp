import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.css'],
})
export class NotificationsComponent implements OnInit {
  constructor() {}

  ngOnInit(): void {}

  // async loadMessages() {
  //   if (!this.user) {
  //     return;
  //   }
  //   this.user = await this.accountService.getAuthenticatedUser(this.user);
  //   this.messageService
  //     .getMessages(this.pageNumber, this.pageSize, this.container)
  //     .subscribe({
  //       next: (response) => {
  //         this.messages = response.result;
  //         this.pagination = response.pagination;
  //       },
  //     });
  // }
  // pageChanged(event: any) {
  //   if (this.pageNumber !== event.page) {
  //     this.pageNumber = event.page;
  //     this.loadMessages();
  //   }
  // }
}
