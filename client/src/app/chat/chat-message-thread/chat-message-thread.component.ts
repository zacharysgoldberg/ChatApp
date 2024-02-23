import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { take } from 'rxjs';
import { ContactModel } from 'src/app/_models/contact.model';
import { MessageModel } from 'src/app/_models/message.model';
import { UserModel } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';
import { ContactService } from 'src/app/_services/contact.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-chat-message-thread',
  templateUrl: './chat-message-thread.component.html',
  styleUrls: ['./chat-message-thread.component.css'],
})
export class ChatMessageThreadComponent implements OnInit {
  user: UserModel | undefined; // for authentication only
  @Input() messageThread: MessageModel[] = [];
  @ViewChild('messageForm') messageForm?: NgForm;
  @Input() contact: ContactModel | undefined;
  messageContent = '';
  // pageNumber = 1;
  // pageSize = 5;
  // container = 'Unread';

  constructor(
    public accountService: AccountService,
    private messageService: MessageService,
    private contactService: ContactService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.user = user;
        }
      },
    });

    this.contact = this.messageService.getContact();

    if (this.contact) this.loadMessageThread(this.contact.id);
  }

  async loadMessageThread(recipientId: number) {
    if (!this.user) {
      return;
    }

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.contactService.getContact(recipientId).subscribe({
      next: (contact) => (this.contact = contact),
    });

    this.messageService.createMessageThread(recipientId).subscribe({
      next: (messageThread) => {
        if (messageThread) this.messageThread = messageThread;
      },
      error: (error) => {
        console.error('Error fetching message thread:', error);
      },
    });
  }

  async sendMessage() {
    if (!this.user) return;

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (!this.contact) return;

    this.messageService
      .sendMessage(this.contact.id, this.messageContent)
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
