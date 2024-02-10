import { HttpClient, HttpHeaders } from '@angular/common/http';
import { LoginModel } from '../_models/login.model';
import { UserModel } from '../_models/user.model';
import { RegisterModel } from '../_models/register.model';
import { BehaviorSubject, Observable, Subject, map } from 'rxjs';
import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { ResetPasswordModel } from '../_models/resetPassword.model';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  apiUrl = environment.apiUrl;

  private currentUserSource = new BehaviorSubject<UserModel | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient, private jwtHelper: JwtHelperService) {
    const userString = JSON.stringify(localStorage.getItem('username'));

    if (userString) {
      const user: UserModel = JSON.parse(userString);

      this.setCurrentUserSource(user);
      this.currentUser$ = this.currentUserSource.asObservable();
    }
  }

  login(credentials: LoginModel) {
    // console.log(this.apiUrl);
    return this.http
      .post<UserModel>(this.apiUrl + 'account/login', credentials)
      .pipe(
        map((response: UserModel) => {
          const user = response;
          if (user) {
            this.setUserAccess(user);
            this.currentUserSource.next(user);
          }
        })
      );
  }

  register(registration: RegisterModel) {
    return this.http
      .post<UserModel>(this.apiUrl + 'account/register', registration)
      .pipe(
        map((user) => {
          if (user) {
            this.setUserAccess(user);
            this.currentUserSource.next(user);
          }
          // return user;
        })
      );
  }

  logout() {
    // const username = this.getUsername();

    localStorage.removeItem('user');
    this.currentUserSource.next(null);

    // return this.http.post(this.apiUrl + `account/revoke/${username}`, model);
  }

  setUserAccess(user: UserModel): void {
    localStorage.setItem('user', JSON.stringify(user));
  }

  setCurrentUserSource(user: UserModel): void {
    this.currentUserSource.next(user);
  }

  async tryRefreshingTokens(token: string | null): Promise<boolean> {
    const userString = localStorage.getItem('user');

    if (!userString) return false;

    const user: UserModel = JSON.parse(userString);
    const username = user.username;
    const refreshToken = user.refreshToken;

    if (!token || !refreshToken || !username) return false;

    const credentials = JSON.stringify({
      username: username,
      accessToken: token,
      refreshToken: refreshToken,
    });

    let isRefreshSuccess: boolean;

    const refreshRes = await new Promise<UserModel>((resolve, reject) => {
      this.http
        .post<UserModel>(this.apiUrl + 'account/refresh-token', credentials, {
          headers: new HttpHeaders({
            'Content-Type': 'application/json',
          }),
        })
        .subscribe({
          next: (res: UserModel) => resolve(res),
          error: (_) => {
            reject;
            isRefreshSuccess = false;
          },
        });
    });

    user.accessToken = refreshRes.accessToken;
    user.refreshToken = refreshRes.refreshToken;

    localStorage.setItem('user', JSON.stringify(user));
    isRefreshSuccess = true;

    return isRefreshSuccess;
  }

  async getAccessToken(currentUser: UserModel): Promise<string> {
    if (
      !this.isUserAuthenticated() ||
      this.jwtHelper.isTokenExpired(currentUser.accessToken)
    ) {
      const isRefreshSuccess = await this.tryRefreshingTokens(
        currentUser.accessToken
      );
      if (!isRefreshSuccess) return currentUser.accessToken;

      const userString = localStorage.getItem('user');

      if (!userString) return currentUser.accessToken;

      const newUser: UserModel = JSON.parse(userString!);
      return newUser.accessToken;
    }

    return currentUser.accessToken;
  }

  getUsername() {
    if (this.isUserAuthenticated()) {
      const userString = localStorage.getItem('user');

      if (!userString) return null;

      const user: UserModel = JSON.parse(userString);
      const username = user.username;
      return username;
    }
    return null;
  }

  isUserAuthenticated(): boolean {
    const userString = localStorage.getItem('user');

    if (!userString) return false;

    const user: UserModel = JSON.parse(userString);
    const token = user.accessToken;

    if (token && !this.jwtHelper.isTokenExpired(token)) {
      return true;
    }
    return false;
  }
}
