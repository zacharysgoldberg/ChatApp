import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
})
export class ForgotPasswordComponent implements OnInit {
  forgotPasswordForm: FormGroup = new FormGroup({});
  showAlert: boolean = false;
  validationErrors: string[] | undefined;

  @Output() cancelForgotPassword = new EventEmitter();

  constructor(
    private accountService: AccountService,
    private router: Router,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  forgotPassword() {
    if (this.forgotPasswordForm.valid) {
      const forgotPasswordModel = this.forgotPasswordForm.value;

      this.accountService.forgotPassword(forgotPasswordModel).subscribe({
        next: () => {
          this.showAlert = true;
        },
        error: (error) => {
          if (error.status === 404) {
            console.log(error.status);
            this.showAlert = true;
          } else {
            this.validationErrors = error;
          }
        },
      });
    }
    return;
  }
}
