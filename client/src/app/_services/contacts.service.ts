import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.development';
import { ContactModel } from '../_models/contact.model';
import { UserModel } from '../_models/user.model';
import { MemberModel } from '../_models/member.mode';

@Injectable({
  providedIn: 'root',
})
export class ContactsService {
  baseUrl = environment.apiUrl;
  user: UserModel | undefined;
  contact: ContactModel | undefined | null;
  member: MemberModel | undefined;

  constructor(private http: HttpClient) {}

  addContact(contact: ContactModel) {
    return this.http.post<ContactModel>(
      this.baseUrl + 'users/contacts',
      contact
    );
  }

  getContacts() {
    return this.http.get<ContactModel[]>(this.baseUrl + 'users/contacts');
  }

  getContact(contactUsername: string) {
    return this.http.get<ContactModel>(
      this.baseUrl + 'users/contacts/' + contactUsername
    );
  }

  getMember(username: string) {
    return this.http.get<MemberModel>(
      this.baseUrl + 'users/members/' + username
    );
  }

  updateMember() {
    return this.http.put(this.baseUrl + 'users', this.member);
  }
}
