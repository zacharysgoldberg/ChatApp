import { Component, OnInit } from '@angular/core';
import { Contact } from 'src/app/_models/contact.model';
import { ContactsService } from 'src/app/_services/contacts.service';

@Component({
  selector: 'app-contact-list',
  templateUrl: './contact-list.component.html',
  styleUrls: ['./contact-list.component.css'],
})
export class ContactListComponent implements OnInit {
  contacts: Contact[] = [];

  constructor(private contactsService: ContactsService) {}

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts() {
    this.contactsService.getContacts().subscribe({
      next: (contacts) => (this.contacts = contacts),
    });
  }
}
