import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.development';
import { ContactModel } from '../_models/contact.model';
import { UserModel } from '../_models/user.model';
import { map, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  apiUrl = environment.apiUrl;

  contacts: ContactModel[] = [];

  user: UserModel | undefined;
  contact: ContactModel | undefined | null;

  constructor(private http: HttpClient) {}

  addContact(contactUsername: string) {
    const contactUsernameDTO = { username: contactUsername };

    return this.http.post<ContactModel>(
      this.apiUrl + 'contacts',
      contactUsernameDTO
    );
  }

  getContacts() {
    if (this.contacts.length > 0) return of(this.contacts);

    return this.http.get<ContactModel[]>(this.apiUrl + 'contacts').pipe(
      map((contacts) => {
        this.contacts = contacts;
        return this.contacts;
      })
    );
  }

  getContact(contactId: number) {
    const loadedContact = this.contacts.find((c) => c.id == contactId);

    if (loadedContact) return of(loadedContact);

    return this.http.get<ContactModel>(this.apiUrl + `contacts/${contactId}`);
  }

  removeContact(contactId: number) {
    return this.http.post(
      this.apiUrl + `contacts/delete/${contactId}`,
      this.contact
    );
  }
}
