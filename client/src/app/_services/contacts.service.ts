import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.development';
import { ContactModel } from '../_models/contact.model';
import { UserModel } from '../_models/user.model';

@Injectable({
  providedIn: 'root',
})
export class ContactsService {
  baseUrl = environment.apiUrl;
  user: UserModel | undefined;
  contact: ContactModel | undefined;

  constructor(private http: HttpClient) {}

  addContact(contact: ContactModel) {
    return this.http.post<ContactModel>(
      this.baseUrl + 'users/contacts',
      contact,
      this.getHttpOptions()
    );
  }

  getContacts() {
    return this.http.get<ContactModel[]>(
      this.baseUrl + 'users/contacts',
      this.getHttpOptions()
    );
  }

  getContact(contactUsername: string) {
    return this.http.get<ContactModel>(
      this.baseUrl + 'users/contacts/' + contactUsername,
      this.getHttpOptions()
    );
  }

  getHttpOptions() {
    const userString = JSON.stringify(localStorage.getItem('username'));
    if (!userString) return;

    const user = JSON.parse(userString);
    return {
      headers: new HttpHeaders({
        Authorization: 'Bearer ' + user.accessToken,
      }),
    };
  }
}
