import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ChangePasswordModel } from 'src/app/_models/changePassword.model';
import { MemberUpdateModel } from 'src/app/_models/memberUpdate.model';
import { AccountService } from 'src/app/_services/account.service';
import { MemberService } from 'src/app/_services/member.service';

@Component({
  selector: 'app-profile-edit',
  templateUrl: './profile-edit.component.html',
  styleUrls: ['./profile-edit.component.css'],
})
export class ProfileEditComponent implements OnInit {
  @Output() cancelEdit = new EventEmitter();
  @Input() editField: string | undefined;
  memberUpdate: MemberUpdateModel = {
    id: 0,
    userName: '',
    email: '',
    photoUrl: '',
  };

  changePassword: ChangePasswordModel = {
    currentPassword: '',
    password: '',
    confirmPassword: '',
  };

  constructor(
    private accountService: AccountService,
    private memberService: MemberService,
    private router: Router
  ) {}

  ngOnInit(): void {}

  cancel() {
    this.cancelEdit.emit(false);
  }

  save(editField: string) {
    let updateRequest;

    if (editField === 'Password')
      updateRequest = this.memberService.changePassword(this.changePassword);
    else if (editField === 'Username')
      updateRequest = this.memberService.updateUsername(this.memberUpdate);
    else if (editField === 'Email')
      updateRequest = this.memberService.updateEmail(this.memberUpdate);
    else return;

    updateRequest.subscribe({
      next: (response) => {
        console.log(response);
        this.accountService.logout();
        this.router.navigateByUrl('/');
      },
      error: (err) => {
        console.error('Error updating profile:', err);
      },
    });
  }
}
