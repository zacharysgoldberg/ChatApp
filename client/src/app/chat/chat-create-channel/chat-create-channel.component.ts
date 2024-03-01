import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ContactModel } from '../../_models/contact.model';
import { ContactService } from 'src/app/_services/contact.service';
import { CreateGroupMessageModel } from 'src/app/_models/createGroupMessage.model';
import { MessageService } from 'src/app/_services/message.service';
import { UserModel } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';
import { Router } from '@angular/router';
import { take } from 'rxjs';

@Component({
  selector: 'app-chat-create-channel',
  templateUrl: './chat-create-channel.component.html',
  styleUrls: ['./chat-create-channel.component.css'],
})
export class ChatCreateChannelComponent implements OnInit {
  user?: UserModel;
  contacts: ContactModel[] = []; // List of user's contacts
  addedContacts: ContactModel[] = []; // List of contacts added to the channel
  createGroupMessageModel: CreateGroupMessageModel = {
    channelName: '',
    contactIds: [],
  };
  @Output() cancelEdit = new EventEmitter();

  constructor(
    private accountService: AccountService,
    private contactService: ContactService,
    private messageService: MessageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.user = user;
          this.loadContacts();
        }
      },
    });
  }

  async loadContacts() {
    if (!this.user) return;

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.contactService.getContacts().subscribe({
      next: (contacts) => (this.contacts = contacts),
    });
  }

  toggleContactSelection(contact: ContactModel) {
    // Check if the contact is already added to the channel
    const index = this.addedContacts.findIndex((c) => c.id === contact.id);
    if (index !== -1) {
      // If the contact is already added, remove it
      this.addedContacts.splice(index, 1);
    } else {
      // If the contact is not added, add it
      this.addedContacts.push(contact);
    }
  }

  isSelectedContact(contact: ContactModel): boolean {
    // Check if the contact is already added to the channel
    return this.addedContacts.some((c) => c.id === contact.id);
  }

  removeContact(contact: ContactModel) {
    // Remove the contact from the added contacts list
    const index = this.addedContacts.findIndex((c) => c.id === contact.id);
    if (index !== -1) {
      this.addedContacts.splice(index, 1);
    }
  }

  async createChannel() {
    if (!this.user) {
      this.cancel();
      return;
    }

    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.createGroupMessageModel.contactIds = this.addedContacts.map(
      (contact) => contact.id
    );

    if (
      this.createGroupMessageModel.contactIds.length > 0 &&
      this.createGroupMessageModel.channelName.length > 0
    ) {
      this.messageService
        .createGroupMessageChannel(this.createGroupMessageModel)
        .subscribe({
          next: () => location.reload(),

          error: (error) => console.log(error),
        });
    }
    return;
  }

  cancel() {
    this.cancelEdit.emit(false);
  }
}
