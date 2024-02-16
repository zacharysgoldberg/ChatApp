import { Component, Input, OnInit } from '@angular/core';
import { MessageModel } from '../_models/message.model';
import { MessageService } from '../_services/message.service';
import { UserModel } from '../_models/user.model';
import { AccountService } from '../_services/account.service';
import { take } from 'rxjs';
import { MemberModel } from '../_models/member.model';
import { Pagination } from '../_models/pagination.mode';
import { ContactModel } from '../_models/contact.model';
import { UserService } from '../_services/user.service';

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

  getMessageClass(message: any): string {
    return message.senderUsername === this.user?.username
      ? 'chat-right'
      : 'chat-left';
  }

  sendMessage(recipientUsername: string, content: string): void {
    this.messageService.sendMessage(recipientUsername, content).subscribe({
      next: (message) => {
        // Message sent successfully, handle any UI updates or notifications
        console.log('Message sent:', message);
      },
      error: (error) => {
        // Handle error, display error message to user or perform any other actions
        console.error('Error sending message:', error);
      },
    });
  }
}
