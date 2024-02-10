import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
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
    let request: Observable<Object>;

    if (editField === 'Password') {
      request = this.memberService.changePassword(this.changePassword);
      this.submitHardUpate(request);
    } else if (editField === 'Username') {
      request = this.memberService.updateUsername(this.memberUpdate);
      this.submitHardUpate(request);
    } else if (editField === 'Email') {
      request = this.memberService.updateEmail(this.memberUpdate);
      this.submitSoftUpdate(request);
    } else if (editField === 'Phone') {
      request = this.memberService.updatePhone(this.memberUpdate);
      this.submitSoftUpdate(request);
    } else return;
  }

  submitHardUpate(hardRequest: Observable<Object>) {
    hardRequest.subscribe({
      next: (response: any) => {
        console.log(response);
        this.accountService.logout();
        this.router.navigateByUrl('/');
      },
      error: (err: any) => {
        console.error('Error updating profile:', err);
      },
    });
  }

  submitSoftUpdate(hardRequest: Observable<Object>) {
    hardRequest.subscribe({
      next: (response: any) => {
        console.log(response);
        location.reload();
      },
      error: (err: any) => {
        console.error('Error updating profile:', err);
      },
    });
  }
}
