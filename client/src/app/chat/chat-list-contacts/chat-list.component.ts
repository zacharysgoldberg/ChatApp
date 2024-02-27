import { Component, Input, OnInit, Output, ViewChild } from '@angular/core';
import { MessageModel } from '../../_models/message.model';
import { MessageService } from '../../_services/message.service';
import { UserModel } from '../../_models/user.model';
import { AccountService } from '../../_services/account.service';
import { Observable, combineLatest, map, of, switchMap, take } from 'rxjs';
import { MemberModel } from '../../_models/member.model';
import { Pagination } from '../../_models/pagination.mode';
import { ContactModel } from '../../_models/contact.model';
import { UserService } from '../../_services/user.service';
import { ContactService } from 'src/app/_services/contact.service';
import { GroupMessageModel } from 'src/app/_models/groupMessage.model';

@Component({
  selector: 'app-chat',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.css'],
})
export class ChatListComponent implements OnInit {
  user: UserModel | undefined; // for authentication only
  contactsWithMessageThreads$: Observable<ContactModel[]> | undefined;
  groupMessageChannelsForUser$: Observable<GroupMessageModel[]> | undefined;
  contactsAndChannels$:
    | Observable<(ContactModel | GroupMessageModel)[]>
    | undefined;
  pagination?: Pagination;
  createNewChannelMode: boolean = false;
  messageThreadEnabled: boolean = false;
  groupMessageChannelEnabled: boolean = false;
  @Output() messageThread: MessageModel[] = [];
  @Output() groupMessageChannel: GroupMessageModel[] = [];
  @Output() contact: ContactModel | undefined;
  @Output() contacts: ContactModel[] = [];

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
          this.sortContactAndChannels();
        }
      },
    });
  }

  sortContactAndChannels() {
    const contacts$ = this.messageService.getContactsWithMessageThread().pipe(
      map((contacts) => {
        // Sort the contacts based on the last message sent/received timestamp
        return contacts.sort((a, b) => {
          const timestampA = a?.lastActive;
          const timestampB = b?.lastActive;
          return timestampA.getTime() - timestampB.getTime();
        });
      })
    );

    const channels$ = this.messageService.getGroupMessageChannelsForUser().pipe(
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

  async selectUser(contactId: number) {
    if (!this.user) return;

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.contactService.getContact(contactId).subscribe({
      next: (contact) => (this.contact = contact),
    });

    this.messageService.getMessageThread(contactId).subscribe({
      next: (messageThread) => {
        this.messageThread = messageThread;
      },
    });

    this.groupMessageChannelEnabled = false;
    this.messageThreadEnabled = true;
  }

  async selectChannel(channelId: string) {
    if (!this.user) return;

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.messageService.getContactsForGroupMessageChannel(channelId).subscribe({
      next: (contacts) => (this.contacts = contacts),
    });

    this.messageService.getGroupMessageChannel(channelId).subscribe({
      next: (groupMessageChannel) =>
        (this.groupMessageChannel = groupMessageChannel),
    });

    this.messageThreadEnabled = false;
    this.groupMessageChannelEnabled = true;
  }

  toggleCreateNewChannelMode() {
    this.createNewChannelMode = !this.createNewChannelMode;
  }

  cancelCreateNewChannelMode(event: boolean) {
    this.createNewChannelMode = event;
  }
}
