import { Component, OnInit, Output } from '@angular/core';
import { MessageModel } from '../../_models/message.model';
import { MessageService } from '../../_services/message.service';
import { UserModel } from '../../_models/user.model';
import { AccountService } from '../../_services/account.service';
import { Observable, combineLatest, map, of, take } from 'rxjs';
import { ContactModel } from '../../_models/contact.model';
import { GroupMessageModel } from 'src/app/_models/groupMessage.model';
import { ContactService } from 'src/app/_services/contact.service';

@Component({
  selector: 'app-chat',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.css'],
})
export class ChatListComponent implements OnInit {
  user?: UserModel;
  contactsAndChannels$:
    | Observable<(ContactModel | GroupMessageModel)[]>
    | undefined;
  messageThreadEnabled: boolean = false;
  groupMessageChannelEnabled: boolean = false;
  createNewChannelMode: boolean = false;

  @Output() messageThread$: Observable<MessageModel[]> | undefined;
  @Output() groupMessageChannel$: Observable<GroupMessageModel[]> | undefined;
  @Output() contact?: ContactModel;
  @Output() contacts: ContactModel[] = [];

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
          this.sortContactsAndChannels();
        }
      },
    });

    if (this.messageService.contact) {
      this.messageThreadEnabled = true;
      this.messageThread$ = this.messageService.getMessageThread();
    }
  }

  notificationLoaded(id: number | string) {
    if (typeof id === 'number') {
      // Notification is for a one-to-one message thread
      this.contactService.getContact(id).subscribe({
        next: (contact) => this.selectContact(contact),
      });
    } else {
      // Notification is for a group message channel
      console.log(id);
      this.selectChannel(id);
    }
  }

  async selectContact(contact: ContactModel) {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (!this.messageService.isHubConnectionEstablished(contact.id.toString()))
      this.messageService.stopMessageHubConnection();

    this.contact = contact;

    if (this.user && this.contact) {
      await this.messageService.createMessageHubConnection(
        this.user,
        this.contact.id
      );
      this.messageService.getMessageThread().subscribe((messageThread) => {
        this.messageThread$ = of(messageThread); // Update messageThread$ when thread is received
      });
    }

    this.messageThread$ = this.messageService.getMessageThread();

    this.groupMessageChannelEnabled = false;
    this.messageThreadEnabled = true;
  }

  async selectChannel(channelId: string) {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (!this.messageService.isHubConnectionEstablished(channelId))
      this.messageService.stopGroupMessageHubConnection();

    this.messageService.getGroupMessageChannelContacts(channelId).subscribe({
      next: (contacts) => {
        // Set the contacts
        this.contacts = contacts;

        if (this.user && this.contacts.length > 0) {
          this.messageService.createGroupMessageHubConnection(this.user, {
            channelId: channelId,
            channelName: '',
            contactIds: this.contacts.map((contact) => contact.id),
          });
        }

        this.messageService
          .getGroupMessageChannel()
          .subscribe((groupMessageChannel) => {
            this.groupMessageChannel$ = of(groupMessageChannel);
          });

        this.messageThreadEnabled = false;
        this.groupMessageChannelEnabled = true;
      },
      error: (error) => {
        console.error('Error fetching contacts:', error);
      },
    });
  }

  sortContactsAndChannels() {
    const contacts$ = this.messageService.getContactsWithMessageThread().pipe(
      map((contacts) =>
        contacts.sort((a, b) => {
          const timeA =
            (typeof a.lastActive === 'string' &&
              new Date(a.lastActive).getTime()) ||
            0;
          const timeB =
            (typeof b.lastActive === 'string' &&
              new Date(b.lastActive).getTime()) ||
            0;
          return timeB - timeA;
        })
      )
    );

    const channels$ = this.messageService.getGroupMessageChannelNames().pipe(
      map((channels) =>
        channels.sort((a, b) => {
          const timeA =
            (typeof a?.createdAt === 'string' &&
              new Date(a.createdAt).getTime()) ||
            0;
          const timeB =
            (typeof b?.createdAt === 'string' &&
              new Date(b.createdAt).getTime()) ||
            0;
          return timeB - timeA;
        })
      )
    );

    this.contactsAndChannels$ = combineLatest([contacts$, channels$]).pipe(
      map(([contacts, channels]) => [...contacts, ...channels])
    );
  }

  getLastActiveTimestamp(
    contactOrChannel: ContactModel | GroupMessageModel
  ): Date {
    if (this.instanceOfGroupMessage(contactOrChannel))
      // It's a channel (GroupMessageModel)
      return contactOrChannel?.createdAt;
    // It's a contact (ContactModel)
    else return contactOrChannel?.lastActive;
  }

  loadCreatedChannel(channelId: string) {
    this.selectChannel(channelId);
  }

  instanceOfGroupMessage(object: any): object is GroupMessageModel {
    return 'createdAt' in object && 'channelId' in object;
  }

  toggleCreateNewChannelMode() {
    this.createNewChannelMode = !this.createNewChannelMode;
  }

  cancelCreateNewChannelMode(event: boolean) {
    this.createNewChannelMode = event;
  }
}
