import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { take } from 'rxjs';
import { ContactModel } from 'src/app/_models/contact.model';
import { GroupMessageModel } from 'src/app/_models/groupMessage.model';
import { UserModel } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';
import { ContactService } from 'src/app/_services/contact.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-chat-group-message-channel',
  templateUrl: './chat-group-message-channel.component.html',
  styleUrls: ['./chat-group-message-channel.component.css'],
})
export class ChatGroupMessageChannelComponent implements OnInit {
  @Input() groupMessageChannel: GroupMessageModel[] = [];
  @Input() contacts: ContactModel[] = [];
  user: UserModel | undefined; // for authentication only
  @ViewChild('messageForm') messageForm?: NgForm;
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

    // this.contact = this.messageService.getContact();

    if (this.groupMessageChannel.length > 0)
      this.loadChannel(this.groupMessageChannel[0].channelId);
  }

  async loadChannel(channelId: string) {
    if (!this.user) return;

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.messageService.getGroupMessageChannel(channelId).subscribe({
      next: (groupMessageChannel) => {
        if (groupMessageChannel) this.groupMessageChannel = groupMessageChannel;
      },
      error: (error) => {
        console.error('Error fetching group message channel:', error);
      },
    });
  }

  async sendMessage() {
    if (!this.user) return;

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.messageService
      .createGroupMessage(
        this.groupMessageChannel[0].channelId,
        this.groupMessageChannel[0].channelName,
        this.messageContent
      )
      .subscribe({
        next: (message) => {
          // Message sent successfully, handle notifications
          // console.log('Message sent:', message);
          this.groupMessageChannel.push(message);
          this.messageForm?.reset();
        },
        error: (error) => {
          console.error('Error sending message:', error);
        },
      });
  }
}
