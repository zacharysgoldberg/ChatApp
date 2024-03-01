import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { MessageModel } from '../_models/message.model';
import { BehaviorSubject, Observable, catchError, map, of, take } from 'rxjs';
import { ContactModel } from '../_models/contact.model';
import { ContactService } from './contact.service';
import { CreateGroupMessageModel } from '../_models/createGroupMessage.model';
import { GroupMessageModel } from '../_models/groupMessage.model';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';
import { UserModel } from '../_models/user.model';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  apiUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private messageHubConnection?: HubConnection;
  private groupMessageHubConnection?: HubConnection;

  private activeConnections: { [id: string]: boolean } = {};

  contact?: ContactModel;
  contactsWithMessageThreads: ContactModel[] = [];
  private messageThreadSource = new BehaviorSubject<MessageModel[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  groupMessageChannelsForUser: GroupMessageModel[] = [];
  contactsForGroupMessageChannel: ContactModel[] = [];
  private groupMessageChannelSrc = new BehaviorSubject<GroupMessageModel[]>([]);
  groupMessageChannel$ = this.groupMessageChannelSrc.asObservable();

  constructor(
    private http: HttpClient,
    private contactService: ContactService
  ) {}

  /*
    If a hub connection is not established for the contact ID, it stops the existing hub connection.
    If a hub connection is established, it does nothing, allowing the existing connection to continue.
  */
  isHubConnectionEstablished(id: string): boolean {
    if (
      !this.activeConnections[id] &&
      this.messageHubConnection != null &&
      this.messageHubConnection.state === HubConnectionState.Connected
    )
      return false;
    return true;
  }

  async createMessageHubConnection(user: UserModel, recipientId: number) {
    if (this.activeConnections[recipientId]) {
      console.log(`Already connected to contact id: ${recipientId}`);
      return;
    }

    this.messageHubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?recipientId=' + recipientId, {
        accessTokenFactory: () => user.accessToken,
      })
      .withAutomaticReconnect()
      .build();

    await this.messageHubConnection
      .start()
      .catch((error) => console.log(error));

    this.activeConnections[recipientId.toString()] = true;

    this.setContactForMessageThread(recipientId);

    this.messageHubConnection.on('ReceiveMessageThread', (messages) => {
      this.messageThreadSource.next(messages);
    });

    this.messageHubConnection.on('NewMessage', (message) => {
      this.messageThread$.pipe(take(1)).subscribe({
        next: (messages) => {
          this.messageThreadSource.next([...messages, message]);
        },
      });
    });
  }

  stopMessageHubConnection() {
    if (this.messageHubConnection) {
      this.messageHubConnection.stop();
      // Extract recipient/channel id from the hub URL
      const id = this.getIdFromHubUrl(this.messageHubConnection.baseUrl);

      // Remove the ID from the active connections
      if (id !== null) {
        delete this.activeConnections[id];
      }
    }
  }

  private getIdFromHubUrl(url: string): number | string | null {
    const urlParams = new URLSearchParams(url.split('?')[1]);
    let id: number | string | null = null;

    if (urlParams.has('recipientId')) {
      id = parseInt(urlParams.get('recipientId')!, 10);
    } else if (urlParams.has('channelId')) {
      id = urlParams.get('channelId')!;
    }
    return id;
  }

  async createMessage(recipientId: number, content: string) {
    return this.messageHubConnection
      ?.invoke('SendMessage', {
        recipientId: recipientId,
        content: content,
      })
      .catch((error) => console.log(error));
  }

  getMessageThread(): Observable<MessageModel[]> {
    return this.messageThread$;
  }

  setContactForMessageThread(recipientId: number) {
    this.contactService.getContact(recipientId).subscribe({
      next: (contact) => (this.contact = contact),
    });
  }

  getContact() {
    if (this.contact) return this.contact;
    return;
  }

  getContactsWithMessageThread() {
    if (this.contactsWithMessageThreads.length > 0)
      return of(this.contactsWithMessageThreads);

    return this.http
      .get<ContactModel[]>(this.apiUrl + 'messages/contacts')
      .pipe(
        map((contacts) => {
          this.contactsWithMessageThreads = contacts;
          return this.contactsWithMessageThreads;
        })
      );
  }

  async createGroupMessageHubConnection(
    user: UserModel,
    createGroupMessage: CreateGroupMessageModel
  ) {
    const channelId = createGroupMessage?.channelId;
    const channelName = createGroupMessage.channelName;
    const contactIds = createGroupMessage.contactIds;

    if (!channelId) return;

    let hubUrlWithParams =
      this.hubUrl +
      `group-message?channelId=${channelId}&channelName=${channelName}&contactIds=${contactIds?.join(
        ','
      )}`;

    if (this.activeConnections[channelId]) {
      console.log(`Already connected to channel id: ${channelId}`);
      return;
    }

    this.groupMessageHubConnection = new HubConnectionBuilder()
      .withUrl(hubUrlWithParams, {
        accessTokenFactory: () => user.accessToken,
      })
      .withAutomaticReconnect()
      .build();

    await this.groupMessageHubConnection
      .start()
      .catch((error) => console.log(error));

    console.log(this.groupMessageHubConnection);

    this.activeConnections[channelId] = true;

    this.groupMessageHubConnection.on(
      'ReceiveGroupMessageChannel',
      (groupMessages) => {
        this.groupMessageChannelSrc.next(groupMessages);
      }
    );

    this.groupMessageHubConnection.on('NewGroupMessage', (groupMessage) => {
      this.groupMessageChannel$.pipe(take(1)).subscribe({
        next: (groupMessages) => {
          this.groupMessageChannelSrc.next([...groupMessages, groupMessage]);
        },
      });
    });
  }

  stopGroupMessageHubConnection() {
    if (this.groupMessageHubConnection) {
      this.groupMessageHubConnection.stop();
      // Extract recipient/channel id from the hub URL
      const id = this.getIdFromHubUrl(this.groupMessageHubConnection.baseUrl);

      // Remove the ID from the active connections
      if (id !== null) {
        delete this.activeConnections[id];
      }
    }
  }

  createGroupMessageChannel(createGroupMessageModel: CreateGroupMessageModel) {
    return this.http.post<GroupMessageModel[]>(
      this.apiUrl + 'groupmessages/channel',
      createGroupMessageModel
    );
  }

  async createGroupMessage(
    channelId: string,
    channelName: string,
    content: string
  ) {
    return this.groupMessageHubConnection
      ?.invoke('SendGroupMessage', {
        channelId: channelId,
        channelName: channelName,
        content: content,
      })
      .catch((error) => console.log(error));
  }

  getGroupMessageChannelNames() {
    if (this.groupMessageChannelsForUser.length > 0)
      return of(this.groupMessageChannelsForUser);

    return this.http
      .get<GroupMessageModel[]>(this.apiUrl + 'groupmessages')
      .pipe(
        map((channels) => {
          this.groupMessageChannelsForUser = channels;
          return this.groupMessageChannelsForUser;
        })
      );
  }

  getGroupMessageChannel(): Observable<GroupMessageModel[]> {
    return this.groupMessageChannel$;
  }

  getContactsForGroupMessageChannel(channelId: string) {
    if (this.contactsForGroupMessageChannel.length > 0)
      return of(this.contactsForGroupMessageChannel);

    return this.http
      .get<ContactModel[]>(this.apiUrl + `groupmessages/contacts/${channelId}`)
      .pipe(
        map((contacts) => {
          this.contactsForGroupMessageChannel = contacts;
          return this.contactsForGroupMessageChannel;
        })
      );
  }
}
