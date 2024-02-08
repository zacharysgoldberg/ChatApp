import { HttpClient, HttpHeaders } from '@angular/common/http';
import { LoginModel } from '../_models/login.model';
import { UserModel } from '../_models/user.model';
import { RegisterModel } from '../_models/register.model';
import { BehaviorSubject, map } from 'rxjs';
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
  private activeUser = new BehaviorSubject<boolean>(false);
  currentUser$ = this.currentUserSource.asObservable();

  invalidLogin: boolean | undefined;

  constructor(private http: HttpClient, private jwtHelper: JwtHelperService) {
    const userString = localStorage.getItem('currentUser');

    if (userString) {
      const user: UserModel = JSON.parse(userString);

      this.setCurrentUserSource(user);
      this.currentUser$ = this.currentUserSource.asObservable();
    }
  }

  login(credentials: LoginModel) {
    console.log(this.apiUrl);
    return this.http
      .post<UserModel>(this.apiUrl + 'account/login', credentials)
      .pipe(
        map((response: UserModel) => {
          const user = response;
          if (user) {
            this.setUserAccess(user);
            this.currentUserSource.next(user);
            this.activeUser.next(true);
            console.log(this.activeUser);
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

    localStorage.removeItem('username');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.currentUserSource.next(null);
    this.activeUser.next(false);

    // return this.http.post(this.apiUrl + `account/revoke/${username}`, model);
  }

  setUserAccess(user: UserModel): void {
    const username = user.username;
    const accessToken = user.accessToken;
    const refreshToken = user.refreshToken;
    localStorage.setItem('username', username);
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  setCurrentUserSource(user: UserModel): void {
    this.currentUserSource.next(user);
  }

  async tryRefreshingTokens(token: string | null): Promise<boolean> {
    const refreshToken = localStorage.getItem('refreshToken');
    const username = localStorage.getItem('username');
    if (!token || !refreshToken || !username) {
      return false;
    }

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

    localStorage.setItem('accessToken', refreshRes.accessToken);
    localStorage.setItem('refreshToken', refreshRes.refreshToken);
    isRefreshSuccess = true;

    return isRefreshSuccess;
  }

  getUsername() {
    if (this.isUserAuthenticated()) return localStorage.getItem('username');
    return null;
  }

  isUserAuthenticated(): boolean {
    const token = localStorage.getItem('accessToken');
    if (token && !this.jwtHelper.isTokenExpired(token)) {
      return true;
    }
    return false;
  }
}
