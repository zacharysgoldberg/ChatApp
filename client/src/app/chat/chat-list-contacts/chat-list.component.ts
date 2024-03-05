import { Component, OnInit, Output } from '@angular/core';
import { MessageModel } from '../../_models/message.model';
import { MessageService } from '../../_services/message.service';
import { UserModel } from '../../_models/user.model';
import { AccountService } from '../../_services/account.service';
import { Observable, combineLatest, map, of, take } from 'rxjs';
import { ContactModel } from '../../_models/contact.model';
import { GroupMessageModel } from 'src/app/_models/groupMessage.model';

@Component({
  selector: 'app-chat',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.css'],
})
export class ChatListComponent implements OnInit {
  user?: UserModel;
  contactsWithMessageThreads$: Observable<ContactModel[]> | undefined;
  usersGroupMessageChannels$: Observable<GroupMessageModel[]> | undefined;
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
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.user = user;
          this.sortContactAndChannels();
        }
      },
    });

    if (this.messageService.contact) {
      this.messageThreadEnabled = true;
      this.messageThread$ = this.messageService.getMessageThread();
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

  sortContactAndChannels() {
    const contacts$ = this.messageService.getContactsWithMessageThread().pipe(
      map((contacts) => {
        // Sort the contacts based on the last message sent/received timestamp
        return contacts.sort((a, b) => {
          const timestampA =
            a.lastActive instanceof Date ? a.lastActive.getTime() : 0;
          const timestampB =
            b.lastActive instanceof Date ? b.lastActive.getTime() : 0;
          return timestampA - timestampB;
        });
      })
    );

    const channels$ = this.messageService.getGroupMessageChannelNames().pipe(
      map((channels) => {
        // Sort the channels based on the latest message sent timestamp
        return channels.sort((a, b) => {
          const timestampA = a?.createdAt;
          const timestampB = b?.createdAt;
          return (
            (timestampB instanceof Date ? timestampA.getTime() : 0) -
            (timestampA instanceof Date ? timestampB.getTime() : 0)
          );
        });
      })
    );

    this.contactsAndChannels$ = combineLatest([contacts$, channels$]).pipe(
      map(([contacts, channels]) => {
        // Combine and sort contacts and channels based on the latest activity timestamp
        const combinedArray = [...contacts, ...channels];
        return combinedArray.sort((a, b) => {
          const timestampA = this.getLastActiveTimestamp(a);
          const timestampB = this.getLastActiveTimestamp(b);
          return (
            (timestampA instanceof Date ? timestampA.getTime() : 0) -
            (timestampB instanceof Date ? timestampB.getTime() : 0)
          );
        });
      })
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
