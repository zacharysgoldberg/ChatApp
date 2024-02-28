import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ContactModel } from 'src/app/_models/contact.model';
import { ContactService } from 'src/app/_services/contact.service';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { Observable, take } from 'rxjs';
import { MessageService } from '../_services/message.service';
import { PresenceService } from '../_services/presence.service';
import { UserModel } from '../_models/user.model';

@Component({
  selector: 'app-contacts',
  templateUrl: './contacts.component.html',
  styleUrls: ['./contacts.component.css'],
})
export class ContactsComponent implements OnInit {
  user?: UserModel;
  contacts$: Observable<ContactModel[]> | undefined;
  contact: ContactModel | undefined;
  contactToAdd: string = '';

  constructor(
    public accountService: AccountService,
    private contactService: ContactService,
    private messageService: MessageService,
    public presenceService: PresenceService,
    private toastr: ToastrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });

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
    this.messageService.setContactForMessageThread(contactId);
    this.router.navigate(['/chat']);
  }
}
