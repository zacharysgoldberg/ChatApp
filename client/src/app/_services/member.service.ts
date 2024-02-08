import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { MemberModel } from '../_models/member.model';
import { HttpClient } from '@angular/common/http';
import { MemberUpdateModel } from '../_models/memberUpdate.model';
import { ChangePasswordModel } from '../_models/changePassword.model';

@Injectable({
  providedIn: 'root',
})
export class MemberService {
  baseUrl = environment.apiUrl;
  member: MemberModel | undefined;

  constructor(private http: HttpClient) {}

  getMember(username: string) {
    return this.http.get<MemberModel>(
      this.baseUrl + `users/members/${username}`
    );
  }

  updateUsername(memberUpdate: MemberUpdateModel) {
    return this.http.put(this.baseUrl + 'users/update-username', memberUpdate);
  }

  updateEmail(memberUpdate: MemberUpdateModel) {
    return this.http.put(this.baseUrl + 'users/update-email', memberUpdate);
  }

  changePassword(changePassword: ChangePasswordModel) {
    return this.http.post(
      this.baseUrl + 'users/reset-password',
      changePassword
    );
  }
}
