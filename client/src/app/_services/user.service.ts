import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { MemberModel } from '../_models/member.model';
import { HttpClient } from '@angular/common/http';
import { MemberUpdateModel } from '../_models/memberUpdate.model';
import { ChangePasswordModel } from '../_models/changePassword.model';
import { FileUploader } from 'ng2-file-upload';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  apiUrl = environment.apiUrl;
  member: MemberModel | undefined;

  constructor(private http: HttpClient) {}

  getMember(username: string) {
    return this.http.get<MemberModel>(
      this.apiUrl + `users/members/${username}`
    );
  }

  updateUsername(memberUpdate: MemberUpdateModel) {
    return this.http.put(this.apiUrl + 'users/update-username', memberUpdate);
  }

  updateEmail(memberUpdate: MemberUpdateModel) {
    return this.http.put(this.apiUrl + 'users/update-email', memberUpdate);
  }

  updatePhone(memberUpdate: MemberUpdateModel) {
    return this.http.put(this.apiUrl + 'users/update-phone', memberUpdate);
  }

  changePassword(changePassword: ChangePasswordModel) {
    return this.http.post(
      this.apiUrl + 'users/update-password',
      changePassword
    );
  }

  deletePhoto() {
    return this.http.delete(this.apiUrl + 'users/delete-photo');
  }
}
