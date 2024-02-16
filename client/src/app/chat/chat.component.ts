import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { MessageModel } from '../_models/message.model';
import { MessageService } from '../_services/message.service';
import { UserModel } from '../_models/user.model';
import { AccountService } from '../_services/account.service';
import { take } from 'rxjs';
import { MemberModel } from '../_models/member.model';
import { Pagination } from '../_models/pagination.mode';
import { ContactModel } from '../_models/contact.model';
import { UserService } from '../_services/user.service';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css'],
})
export class ChatComponent implements OnInit {
  contactsWithMessageThreads?: ContactModel[];
  messageThread: MessageModel[] = [];
  pagination?: Pagination;
  selectedUser: ContactModel | null = null;
  user: UserModel | undefined;
  member: MemberModel | undefined;
  @ViewChild('messageForm') messageForm?: NgForm;
  messageContent = '';
  // pageNumber = 1;
  // pageSize = 5;
  // container = 'Unread';

  constructor(
    public accountService: AccountService,
    private userService: UserService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.user = user;
          this.userService.getMember(this.user.username).subscribe({
            next: (member) => (this.member = member),
          });
          this.loadContactsWithMessageThread();
        }
      },
    });
  }

  async loadContactsWithMessageThread() {
    if (!this.user) {
      return;
    }

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    // Fetch users with message threads for the current user
    this.messageService.getContactsWithMessageThread().subscribe({
      next: (contacts) => {
        this.contactsWithMessageThreads = contacts;
      },
      error: (error) => {
        console.error('Error fetching contacts with message threads:', error);
      },
    });
  }

  async loadMessageThread(id: number) {
    if (!this.user) {
      return;
    }

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.messageService.getMessageThread(id).subscribe({
      next: (messageThread) => {
        this.messageThread = messageThread;
      },
      error: (error) => {
        console.error('Error fetching message thread:', error);
      },
    });
  }

  selectUser(contact: ContactModel) {
    this.selectedUser = contact;
    this.loadMessageThread(contact.id);
  }

  async sendMessage() {
    if (!this.user) return;

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (!this.selectedUser) return;

    this.messageService
      .sendMessage(this.selectedUser.id, this.messageContent)
      .subscribe({
        next: (message) => {
          // Message sent successfully, handle notifications
          // console.log('Message sent:', message);
          this.messageThread.push(message);
          this.messageForm?.reset();
        },
        error: (error) => {
          console.error('Error sending message:', error);
        },
      });
  }
}
