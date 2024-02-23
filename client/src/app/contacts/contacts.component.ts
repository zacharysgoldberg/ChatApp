import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ContactModel } from 'src/app/_models/contact.model';
import { ContactService } from 'src/app/_services/contact.service';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-contacts',
  templateUrl: './contacts.component.html',
  styleUrls: ['./contacts.component.css'],
})
export class ContactsComponent implements OnInit {
  contacts$: Observable<ContactModel[]> | undefined;
  contact: ContactModel | undefined;
  contactToAdd: string = '';

  constructor(
    public accountService: AccountService,
    private contactService: ContactService,
    private messageService: MessageService,
    private toastr: ToastrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.contacts$ = this.contactService.getContacts();
    if (this.contactService.contacts.length > 0)
      return this.loadContact(this.contactService.contacts[0].id);
  }

  loadContact(contactId: number) {
    this.contactService.getContact(contactId).subscribe({
      next: (contact) => (this.contact = contact),
    });
  }

  addContact() {
    this.contactService.addContact(this.contactToAdd).subscribe({
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
    this.contactService.removeContact(contactId).subscribe({
      next: (_) => location.reload(),
    });
  }

  message(contactId: number) {
    this.messageService.createMessageThread(contactId);
    this.router.navigate(['/chat']);
  }
}
