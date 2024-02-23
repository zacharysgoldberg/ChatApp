import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { MessageModel } from '../_models/message.model';
import { Observable, catchError } from 'rxjs';
import { ContactModel } from '../_models/contact.model';
import { Router } from '@angular/router';
import { ContactService } from './contact.service';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  apiUrl = environment.apiUrl;
  contact: ContactModel | undefined;

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

  getContact() {
    if (this.contact) return this.contact;
    return;
  }

  getContactsWithMessageThread() {
    return this.http.get<ContactModel[]>(this.apiUrl + 'messages/contacts');
  }

  getMessageThread(recipientId: number) {
    return this.http.get<MessageModel[]>(
      this.apiUrl + 'messages/' + recipientId
    );
  }

  createMessageThread(recipientId: number) {
    this.contactService.getContact(recipientId).subscribe({
      next: (contact) => (this.contact = contact),
    });

    return this.http.post<MessageModel[]>(this.apiUrl + 'messages/thread', {
      recipientId: recipientId,
    });
  }

  sendMessage(recipientId: number, content: string): Observable<MessageModel> {
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
}
