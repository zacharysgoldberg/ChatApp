import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, take } from 'rxjs';
import { ChangePasswordModel } from 'src/app/_models/changePassword.model';
import { MemberUpdateModel } from 'src/app/_models/memberUpdate.model';
import { UserModel } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';
import { UserService } from 'src/app/_services/user.service';

@Component({
  selector: 'app-profile-edit',
  templateUrl: './profile-edit.component.html',
  styleUrls: ['./profile-edit.component.css'],
})
export class ProfileEditComponent implements OnInit {
  @Input() editField: string | undefined;
  user: UserModel | undefined;
  changePassword: ChangePasswordModel = {
    currentPassword: '',
    password: '',
    confirmPassword: '',
  };
  memberUpdate: MemberUpdateModel = {
    id: 0,
    userName: '',
    email: '',
    photoUrl: '',
    phoneNumber: '',
  };
  @Output() cancelEdit = new EventEmitter();

  constructor(
    private accountService: AccountService,
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });
  }

  cancel() {
    this.cancelEdit.emit(false);
  }

  async save(editField: string) {
    // Ensure the user is authenticated before making a new request
    if (!this.user) {
      this.cancel();
      return;
    }
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    let request: Observable<Object>;

    if (this.memberUpdate) {
      switch (editField) {
        case 'Password':
          if (this.changePassword) {
            request = this.userService.changePassword(this.changePassword);
            this.submitHardUpate(request);
          }
          break;
        case 'Username':
          request = this.userService.updateUsername(this.memberUpdate);
          this.submitHardUpate(request);
          break;
        case 'Email':
          request = this.userService.updateEmail(this.memberUpdate);
          this.submitSoftUpdate(request);
          break;
        case 'Phone Number':
          request = this.userService.updatePhone(this.memberUpdate);
          this.submitSoftUpdate(request);
          break;
        default:
          console.error('Invalid editField:', editField);
          break;
      }
    }
    location.reload();
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
        this.cancel();
        location.reload();
        this.router.navigateByUrl('/profile');
      },
      error: (err: any) => {
        console.error('Error updating profile:', err);
      },
    });
  }
}
