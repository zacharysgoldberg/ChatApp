import { EmailValidator } from '@angular/forms';

export interface RegisterModel {
  email: string;
  password: string;
  confirmPassword: string;
}
