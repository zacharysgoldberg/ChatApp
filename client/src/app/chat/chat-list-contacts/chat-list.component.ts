import { Component, Input, OnInit, Output, ViewChild } from '@angular/core';
import { MessageModel } from '../../_models/message.model';
import { MessageService } from '../../_services/message.service';
import { UserModel } from '../../_models/user.model';
import { AccountService } from '../../_services/account.service';
import { take } from 'rxjs';
import { MemberModel } from '../../_models/member.model';
import { Pagination } from '../../_models/pagination.mode';
import { ContactModel } from '../../_models/contact.model';
import { UserService } from '../../_services/user.service';
import { ContactService } from 'src/app/_services/contact.service';

@Component({
  selector: 'app-chat',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.css'],
})
export class ChatListComponent implements OnInit {
  user: UserModel | undefined; // for authentication only
  contactsWithMessageThreads?: ContactModel[];
  @Output() messageThread: MessageModel[] = [];
  @Output() contact: ContactModel | undefined;
  pagination?: Pagination;

  constructor(
    public accountService: AccountService,
    private contactService: ContactService,
    private messageService: MessageService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.user = user;
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

  selectUser(contactId: number) {
    this.contactService.getContact(contactId).subscribe({
      next: (contact) => (this.contact = contact),
    });

    this.messageService.getMessageThread(contactId).subscribe({
      next: (messageThread) => {
        this.messageThread = messageThread;
      },
    });
  }
}
