import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.development';
import { ContactModel } from '../_models/contact.model';
import { map, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  apiUrl = environment.apiUrl;

  contacts: ContactModel[] = [];
  contact: ContactModel | undefined | null;

  constructor(private http: HttpClient) {}

  addContact(contactUsername: string) {
    const contactUsernameDTO = { usernameOrEmail: contactUsername };

    return this.http.post<ContactModel>(
      this.apiUrl + 'contacts',
      contactUsernameDTO
    );
  }

  getContacts() {
    return this.http.get<ContactModel[]>(this.apiUrl + 'contacts');
  }

  getContact(contactId: number) {
    const loadedContact = this.contacts.find((c) => c.id == contactId);

    if (loadedContact) return of(loadedContact);

    return this.http.get<ContactModel>(this.apiUrl + `contacts/${contactId}`);
  }

  removeContact(contactId: number) {
    return this.http.delete(this.apiUrl + `contacts/delete/${contactId}`).pipe(
      map(() => {
        // Remove the contact from the client-side list
        this.contacts = this.contacts.filter((c) => c.id !== contactId);
      })
    );
  }
}
