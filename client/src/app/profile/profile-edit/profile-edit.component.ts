import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, take, throwError } from 'rxjs';
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
  changePasswordForm: FormGroup = new FormGroup({});
  validationErrors: string[] | undefined;
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
    private fb: FormBuilder,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });

    this.initializeForm();
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

    // Await the result of getAuthenticatedUser before proceeding
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    let request: Observable<Object>;

    if (this.memberUpdate) {
      switch (editField) {
        case 'Password':
          if (this.changePasswordForm.valid) {
            const changePasswordModel = this.changePasswordForm.value;

            this.userService.changePassword(changePasswordModel).subscribe({
              next: () => {
                this.submitHardUpate(request);
              },
              error: (error) => {
                // Throw validation errors
                if (error.status === 400) {
                  this.validationErrors = error.error;
                  return;
                } else {
                  // Rethrow other errors
                  return this.toastr.error(error);
                }
              },
            });
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

  initializeForm() {
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(15),
          this.validatePassword,
        ],
      ],
      confirmPassword: [
        '',
        [Validators.required, this.matchValues('newPassword')],
      ],
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value == control.parent?.get(matchTo)?.value
        ? null
        : { notMatching: true };
    };
  }

  validatePassword: ValidatorFn = (control: AbstractControl) => {
    const passwordRegex =
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,15}$/;
    const valid = passwordRegex.test(control.value);
    return valid ? null : { invalidPassword: true };
  };
}
