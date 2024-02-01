import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.development';
import { Contact } from '../_models/contact.model';
import { UserModel } from '../_models/user.model';

@Injectable({
  providedIn: 'root',
})
export class ContactsService {
  baseUrl = environment.apiUrl;
  user: UserModel | undefined;

  constructor(private http: HttpClient) {}

  addContact() {}

  getContacts() {
    return this.http.get<Contact[]>(
      this.baseUrl + 'users/contacts',
      this.getHttpOptions()
    );
  }

  getContact(contactUsername: string) {
    return this.http.get<Contact>(
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
