import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { MessageModel } from '../_models/message.model';
import { BehaviorSubject, Observable, catchError, map, of, take } from 'rxjs';
import { ContactModel } from '../_models/contact.model';
import { ContactService } from './contact.service';
import { CreateGroupMessageModel } from '../_models/createGroupMessage.model';
import { GroupMessageModel } from '../_models/groupMessage.model';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { UserModel } from '../_models/user.model';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  apiUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;

  contact?: ContactModel;
  contactsWithMessageThreads: ContactModel[] = [];
  private messageThreadSource = new BehaviorSubject<MessageModel[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  groupMessageChannelsForUser: GroupMessageModel[] = [];
  contactsForGroupMessageChannel: ContactModel[] = [];

  constructor(
    private http: HttpClient,
    private contactService: ContactService
  ) {}

  async createHubConnection(user: UserModel, recipientId: number) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?recipientId=' + recipientId, {
        accessTokenFactory: () => user.accessToken,
      })
      .withAutomaticReconnect()
      .build();

    await this.hubConnection.start().catch((error) => console.log(error));

    this.setContactForMessageThread(recipientId);

    this.hubConnection.on('ReceiveMessageThread', (messages) => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on('NewMessage', (message) => {
      this.messageThread$.pipe(take(1)).subscribe({
        next: (messages) => {
          this.messageThreadSource.next([...messages, message]);
        },
      });
    });
  }

  stopHubConnection() {
    if (this.hubConnection) this.hubConnection.stop();
  }

  async createMessage(recipientId: number, content: string) {
    return this.hubConnection
      ?.invoke('SendMessage', {
        recipientId: recipientId,
        content: content,
      })
      .catch((error) => console.log(error));
  }

  setContactForMessageThread(recipientId: number) {
    this.contactService.getContact(recipientId).subscribe({
      next: (contact) => (this.contact = contact),
    });
  }

  getMessageThread(): Observable<MessageModel[]> {
    return this.messageThread$;
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

  // getMessageThread(recipientId: number) {
  //   this.setContactForMessageThread(recipientId);

  //   return this.http.get<MessageModel[]>(
  //     this.apiUrl + 'messages/' + recipientId
  //   );
  // }

  createGroupMessageChannel(createGroupMessageModel: CreateGroupMessageModel) {
    return this.http.post<GroupMessageModel[]>(
      this.apiUrl + 'groupmessages/channel',
      createGroupMessageModel
    );
  }

  createGroupMessage(channelId: string, channelName: string, content: string) {
    return this.http.post<GroupMessageModel>(this.apiUrl + 'groupmessages', {
      channelId: channelId,
      channelName: channelName,
      content: content,
    });
  }

  getGroupMessageChannelsForUser() {
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

  getGroupMessageChannel(channelId: string) {
    return this.http.get<GroupMessageModel[]>(
      this.apiUrl + `groupmessages/${channelId}`
    );
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
