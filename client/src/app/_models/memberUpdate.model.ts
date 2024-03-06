import { EmailValidator } from '@angular/forms';

export interface MemberUpdateModel {
  id: number;
  userName: string;
  email: string;
  photoUrl: string;
  phoneNumber: string;
}
