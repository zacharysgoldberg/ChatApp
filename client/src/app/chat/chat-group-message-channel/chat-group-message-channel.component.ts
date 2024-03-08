import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { take } from 'rxjs';
import { ContactModel } from 'src/app/_models/contact.model';
import { CreateGroupMessageModel } from 'src/app/_models/createGroupMessage.model';
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
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;
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
  }

  ngOnDestroy(): void {
    this.messageService.stopGroupMessageHubConnection();
  }

  async sendMessage() {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (!this.contacts) return;

    if (this.groupMessageChannel.length < 1) return;

    const channelId = this.groupMessageChannel[0].channelId;
    const channelName = this.groupMessageChannel[0].channelName;

    const createGroupMessage: CreateGroupMessageModel = {
      channelId: channelId,
      channelName: channelName,
      content: this.messageContent,
      contactIds: this.contacts.map((contact) => contact.id),
    };

    if (channelId && channelName) {
      this.messageService.createGroupMessage(createGroupMessage).then(() => {
        this.messageForm?.reset();
      });

      this.scrollToBottom();
    }
  }

  onEnterSendMessage(event: Event) {
    // Cast the event to KeyboardEvent
    const keyboardEvent = event as KeyboardEvent;
    // Check if the Enter key was pressed and if the Shift key was not pressed (to avoid creating new lines)
    if (keyboardEvent.key === 'Enter' && !keyboardEvent.shiftKey) {
      // Prevent the default behavior of the Enter key (which is creating a new line)
      keyboardEvent.preventDefault();
      // Call the sendMessage method
      this.sendMessage();
    }
  }

  scrollToBottom(): void {
    try {
      this.scrollContainer.nativeElement.scrollTop =
        this.scrollContainer.nativeElement.scrollHeight;
    } catch (err) {
      console.error(err);
    }
  }
}
