import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { MessageModel } from '../_models/message.model';
import { Observable, catchError, map, of } from 'rxjs';
import { ContactModel } from '../_models/contact.model';
import { ContactService } from './contact.service';
import { CreateGroupMessageModel } from '../_models/createGroupMessage.model';
import { GroupMessageModel } from '../_models/groupMessage.model';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  apiUrl = environment.apiUrl;
  contact: ContactModel | undefined;
  contactsWithMessageThreads: ContactModel[] = [];
  groupMessageChannelsForUser: GroupMessageModel[] = [];
  contactsForGroupMessageChannel: ContactModel[] = [];

  constructor(
    private http: HttpClient,
    private contactService: ContactService
  ) {}

  // getMessages(pageNumber: number, pageSize: number, container: string) {
  //   let params = getPaginationHeaders(pageNumber, pageSize);

  //   params = params.append('Container', container);
  //   return getPaginatedResult<MessageModel[]>(
  //     this.apiUrl + 'messages',
  //     params,
  //     this.http
  //   );
  // }

  createMessageThread(recipientId: number) {
    this.contactService.getContact(recipientId).subscribe({
      next: (contact) => (this.contact = contact),
    });

    return this.http.post<MessageModel[]>(this.apiUrl + 'messages/thread', {
      recipientId: recipientId,
    });
  }

  createMessage(
    recipientId: number,
    content: string
  ): Observable<MessageModel> {
    return this.http
      .post<MessageModel>(this.apiUrl + 'messages', {
        recipientId: recipientId,
        content: content,
      })
      .pipe(
        catchError((error) => {
          throw error;
        })
      );
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

  getMessageThread(recipientId: number) {
    return this.http.get<MessageModel[]>(
      this.apiUrl + 'messages/' + recipientId
    );
  }

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
