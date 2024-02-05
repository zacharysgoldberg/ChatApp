import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ContactModel } from 'src/app/_models/contact.model';
import { ContactsService } from 'src/app/_services/contacts.service';

@Component({
  selector: 'app-contact-list',
  templateUrl: './contact-list.component.html',
  styleUrls: ['./contact-list.component.css'],
})
export class ContactListComponent implements OnInit {
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
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts() {
    this.contactsService.getContacts().subscribe({
      next: (contacts) => (this.contacts = contacts),
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
}
