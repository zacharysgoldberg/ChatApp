import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MemberModel } from '../../_models/member.model';
import { UserModel } from '../../_models/user.model';
import { AccountService } from '../../_services/account.service';
import { take } from 'rxjs';
import { MemberService } from 'src/app/_services/member.service';

@Component({
  selector: 'app-profile-details',
  templateUrl: './profile-details.component.html',
  styleUrls: ['./profile-details.component.css'],
})
export class ProfileDetailsComponent implements OnInit {
  editMode: boolean = false;
  editField: string | undefined;

  member: MemberModel | undefined;
  user: UserModel | null = null;
  newDisplayName: string = '';

  constructor(
    private accountService: AccountService,
    private memberService: MemberService
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile() {
    const username = this.accountService.getUsername();

    if (!username) return;
    this.memberService.getMember(username).subscribe({
      next: (member) => {
        this.member = member;
      },
    });
  }

  editModeToggle(field: string) {
    this.editMode = !this.editMode;
    this.editField = field;
  }

  cancelEditMode(event: boolean) {
    this.editMode = event;
  }
}
