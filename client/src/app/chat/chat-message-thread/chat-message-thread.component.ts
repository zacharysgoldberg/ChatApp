import {
  Component,
  ElementRef,
  Input,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { take } from 'rxjs';
import { ContactModel } from 'src/app/_models/contact.model';
import { MessageModel } from 'src/app/_models/message.model';
import { UserModel } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-chat-message-thread',
  templateUrl: './chat-message-thread.component.html',
  styleUrls: ['./chat-message-thread.component.css'],
})
export class ChatMessageThreadComponent implements OnInit, OnDestroy {
  @Input() contact: ContactModel | undefined;
  @Input() messageThread: MessageModel[] = [];

  user?: UserModel;
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;
  messageContent = '';

  constructor(
    public accountService: AccountService,
    public messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });

    if (!this.contact) {
      this.contact = this.messageService.getContact();

      if (this.contact && this.user) {
        if (
          this.messageService.isHubConnectionEstablished(
            this.contact.id.toString()
          )
        )
          this.messageService.stopMessageHubConnection();
        else
          this.messageService.createMessageHubConnection(
            this.user,
            this.contact.id
          );
      } else this.messageService.stopMessageHubConnection();
    }
  }

  ngOnDestroy(): void {
    this.messageService.stopMessageHubConnection();
  }

  async sendMessage() {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (!this.contact) return;

    this.messageService
      .createMessage(this.contact.id, this.messageContent)
      .then(() => {
        this.messageForm?.reset();
      });

    this.scrollToBottom();
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
