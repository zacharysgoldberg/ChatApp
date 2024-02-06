import { Component, OnInit } from '@angular/core';
import { MemberModel } from '../_models/member.mode';
import { UserModel } from '../_models/user.model';
import { AccountService } from '../_services/account.service';
import { ContactModel } from '../_models/contact.model';
import { ContactsService } from '../_services/contacts.service';
import { take } from 'rxjs';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css'],
})
export class ProfileComponent implements OnInit {
  member: MemberModel | undefined;
  user: UserModel | null = null;
  newDisplayName: string = '';

  constructor(
    private accountService: AccountService,
    private contactService: ContactsService
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile() {
    const username = localStorage.getItem('username');

    if (!username) return;
    this.contactService.getMember(username).subscribe({
      next: (member) => {
        this.member = member;
        console.log(this.member);
      },
    });
  }

  saveChanges() {
    this.contactService.updateMember();
  }
}
