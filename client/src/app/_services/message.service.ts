import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { MessageModel } from '../_models/message.model';
import { Observable, catchError } from 'rxjs';
import { ContactModel } from '../_models/contact.model';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);

    params = params.append('Container', container);
    return getPaginatedResult<MessageModel[]>(
      this.apiUrl + 'messages',
      params,
      this.http
    );
  }

  getContactsWithMessageThread() {
    return this.http.get<ContactModel[]>(this.apiUrl + 'messages/contacts');
  }

  getMessageThread(id: number) {
    return this.http.get<MessageModel[]>(this.apiUrl + 'messages/thread/' + id);
  }

  sendMessage(
    recipientUsername: string,
    content: string
  ): Observable<MessageModel> {
    const createMessageDTO: any = {
      recipientUsername,
      content,
    };

    return this.http
      .post<MessageModel>(this.apiUrl + 'messages', createMessageDTO)
      .pipe(
        catchError((error) => {
          throw error;
        })
      );
  }
}
