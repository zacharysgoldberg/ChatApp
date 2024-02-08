import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ContactModel } from 'src/app/_models/contact.model';
import { ContactsService } from 'src/app/_services/contacts.service';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-contacts',
  templateUrl: './contacts.component.html',
  styleUrls: ['./contacts.component.css'],
})
export class ContactsComponent implements OnInit {
  contacts: ContactModel[] = [];
  contact: ContactModel | undefined;
  contactToAdd: string = '';

  constructor(
    public accountService: AccountService,
    private contactsService: ContactsService,
    private toastr: ToastrService,
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

  loadContact(contactId: number) {
    this.contactsService.getContact(contactId).subscribe({
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
        this.toastr.error(error);
      },
    });
  }

  removeContact(contactId: number) {
    this.contactsService.removeContact(contactId).subscribe({
      next: (_) => location.reload(),
    });
  }

  message() {}
}
