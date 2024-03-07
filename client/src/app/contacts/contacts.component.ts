import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ContactModel } from 'src/app/_models/contact.model';
import { ContactService } from 'src/app/_services/contact.service';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { Observable, Subscription, map, of, take } from 'rxjs';
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
  contacts$: Observable<ContactModel[]> = of([]);
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

  async addContact() {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.contactService.addContact(this.contactToAdd).subscribe({
      next: (_) => {
        // Retrieve the updated list of contacts
        this.contacts$ = this.contactService.getContacts();

        this.contacts$.subscribe((contacts) => {
          // Find the newly added contact in the list
          const newlyAddedContact = contacts.find(
            (contact) => contact.userName === this.contactToAdd
          );
        });
      },
      error: (error) => {
        console.log(error);
        this.toastr.error(error);
      },
    });
  }

  async removeContact(contactId: number) {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.contactService.removeContact(contactId).subscribe({
      next: () => {
        // Remove the contact from the client-side list
        this.contacts$ = this.contacts$.pipe(
          map((contacts) => contacts.filter((c) => c.id !== contactId))
        );
      },
      error: (error) => {
        console.error(error);
      },
    });
  }

  async message(contactId: number) {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    await this.messageService.setContactForMessageThread(contactId);
    this.router.navigate(['/chat']);
  }
}
