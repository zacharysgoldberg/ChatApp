import { Component, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup = new FormGroup({});
  validationErrors: string[] | undefined;

  constructor(
    private accountService: AccountService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const email = params['email'];
      let token = params['token'];
      token = token.replace(/ /g, '+');

      this.initializeForm(email, token);
    });
  }

  initializeForm(email: string | null, token: string | null) {
    this.resetPasswordForm = this.fb.group({
      password: [
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
        [Validators.required, this.matchValues('password')],
      ],
      email: [email || '', Validators.required],
      token: [token || '', Validators.required],
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value == control.parent?.get(matchTo)?.value
        ? null
        : { notMatching: true };
    };
  }

  resetPassword() {
    if (this.resetPasswordForm.valid) {
      const resetPasswordModel = this.resetPasswordForm.value;
      console.log(resetPasswordModel.token);
      this.accountService.resetPassword(resetPasswordModel).subscribe({
        next: () => {
          this.router.navigateByUrl('/');
        },
        error: (error) => {
          this.validationErrors = error;
        },
      });
    }
    return;
  }

  validatePassword: ValidatorFn = (control: AbstractControl) => {
    const passwordRegex =
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,15}$/;
    const valid = passwordRegex.test(control.value);
    return valid ? null : { invalidPassword: true };
  };
}
