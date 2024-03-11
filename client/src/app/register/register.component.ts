import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup = new FormGroup({});
  termsOfServiceAgreed: boolean = false;
  validationErrors: string[] | undefined;

  constructor(
    private accountService: AccountService,
    private router: Router,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
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
    });

    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () =>
        this.registerForm.controls['confirmPassword'].updateValueAndValidity(),
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value == control.parent?.get(matchTo)?.value
        ? null
        : { notMatching: true };
    };
  }

  register() {
    if (this.registerForm)
      this.accountService.register(this.registerForm.value).subscribe({
        next: () => {
          this.router.navigateByUrl('/home');
        },
        error: (error) => {
          this.validationErrors = error;
        },
      });
    return;
  }

  validatePassword: ValidatorFn = (control: AbstractControl) => {
    const passwordRegex =
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,15}$/;
    const valid = passwordRegex.test(control.value);
    return valid ? null : { invalidPassword: true };
  };

  toggleTermsOfServiceAggreement() {
    this.termsOfServiceAgreed = !this.termsOfServiceAgreed;
  }
}
