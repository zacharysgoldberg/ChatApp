import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.development';
import { ContactModel } from '../_models/contact.model';
import { UserModel } from '../_models/user.model';

@Injectable({
  providedIn: 'root',
})
export class ContactsService {
  apiUrl = environment.apiUrl;
  user: UserModel | undefined;
  contact: ContactModel | undefined | null;

  constructor(private http: HttpClient) {}

  addContact(contactUsername: string) {
    const contactUsernameDTO = { username: contactUsername };

    return this.http.post<ContactModel>(
      this.apiUrl + 'users/contacts',
      contactUsernameDTO
    );
  }

  getContacts() {
    return this.http.get<ContactModel[]>(this.apiUrl + 'users/contacts');
  }

  getContact(contactId: number) {
    return this.http.get<ContactModel>(
      this.apiUrl + `users/contacts/${contactId}`
    );
  }

  removeContact(contactId: number) {
    return this.http.post(
      this.apiUrl + `users/contacts/delete/${contactId}`,
      this.contact
    );
  }
}
