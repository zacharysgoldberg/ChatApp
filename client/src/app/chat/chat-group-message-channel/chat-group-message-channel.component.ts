import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { take } from 'rxjs';
import { ContactModel } from 'src/app/_models/contact.model';
import { GroupMessageModel } from 'src/app/_models/groupMessage.model';
import { UserModel } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-chat-group-message-channel',
  templateUrl: './chat-group-message-channel.component.html',
  styleUrls: ['./chat-group-message-channel.component.css'],
})
export class ChatGroupMessageChannelComponent implements OnInit {
  @Input() contacts: ContactModel[] = [];
  @Input() groupMessageChannel: GroupMessageModel[] = [];
  user: UserModel | undefined; // for authentication only
  @ViewChild('messageForm') messageForm?: NgForm;
  messageContent = '';

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

    if (!this.contacts) {
      this.messageService
        .getContactsForGroupMessageChannel(
          this.groupMessageChannel[0].channelId
        )
        .subscribe({
          next: (contacts) => (this.contacts = contacts),
        });

      if (this.contacts && this.user) {
        const contactIds: number[] = (this.contacts as ContactModel[]).map(
          (contact) => contact.id
        );
        if (
          !this.messageService.isHubConnectionEstablished(
            this.groupMessageChannel[0].channelId
          )
        )
          this.messageService.stopMessageHubConnection();
        else
          this.messageService.createGroupMessageHubConnection(this.user, {
            channelId: this.groupMessageChannel[0].channelId,
            channelName: this.groupMessageChannel[0].channelName,
            contactIds: contactIds,
          });
      } else this.messageService.stopMessageHubConnection();
    }

    console.log(this.contacts);
  }

  ngOnDestroy(): void {
    this.messageService.stopGroupMessageHubConnection();
  }

  async sendMessage() {
    if (!this.user) return;

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (!this.contacts) return;

    console.log(this.groupMessageChannel);

    if (this.groupMessageChannel.length < 1) return;

    const channelId = this.groupMessageChannel[0].channelId;
    const channelName = this.groupMessageChannel[0].channelName;

    console.log(channelId, channelName);
    if (channelId && channelName) {
      this.messageService
        .createGroupMessage(channelId, channelName, this.messageContent)
        .then(() => {
          this.messageForm?.reset();
        });
    }
  }
}
