import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { RegisterModel } from '../_models/register.model';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  model: any = {};
  registration: RegisterModel = {
    email: '',
    password: '',
    confirmPassword: '',
  };

  constructor(
    private accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {}

  register() {
    this.accountService.register(this.registration).subscribe({
      next: () => {
        this.router.navigateByUrl('/account');
      },
      error: (error) => {
        this.toastr.error(error.error), console.log(error);
      },
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
