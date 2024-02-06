import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ContactModel } from 'src/app/_models/contact.model';
import { ContactsService } from 'src/app/_services/contacts.service';

@Component({
  selector: 'app-contacts',
  templateUrl: './contacts.component.html',
  styleUrls: ['./contacts.component.css'],
})
export class ContactsComponent implements OnInit {
  contacts: ContactModel[] = [];
  contact: ContactModel | undefined;
  contactToAdd: ContactModel = {
    userName: '',
    id: 0,
    email: '',
    lastActive: new Date(),
    photoUrl: '',
  };

  constructor(
    private contactsService: ContactsService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts() {
    this.contactsService.getContacts().subscribe({
      next: (contacts) => (this.contacts = contacts),
    });
  }

  loadContact(contactUsername: string) {
    this.contactsService.getContact(contactUsername).subscribe({
      next: (contact) => (this.contact = contact),
    });
  }

  addContact() {
    this.contactsService.addContact(this.contactToAdd).subscribe({
      next: (_) => {
        location.reload();
      },

      error: (error) => {
        console.log(error);
      },
    });
  }

  message() {}

  removeContact() {}
}
