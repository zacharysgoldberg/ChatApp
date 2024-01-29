import { HttpClient } from '@angular/common/http';
import { LoginModel } from '../_models/login.model';
import { UserModel } from '../_models/user.model';
import { RegisterModel } from '../_models/register.model';
import { BehaviorSubject, map } from 'rxjs';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  baseUrl = 'https://localhost:81/api/';
  private currentUserSource = new BehaviorSubject<UserModel | null>(null);
  private activeUser = new BehaviorSubject<boolean>(false);
  currentUser$ = this.currentUserSource.asObservable();
  invalidLogin: boolean | undefined;

  constructor(private http: HttpClient) {}

  login(credentials: LoginModel) {
    return this.http
      .post<UserModel>(this.baseUrl + 'account/login', credentials)
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
      .post<UserModel>(this.baseUrl + 'account/register', registration)
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

  logout(model: any) {
    const username = this.getUsername();

    return this.http
      .post(this.baseUrl + `account/revoke/${username}`, model)
      .pipe(
        map(() => {
          localStorage.removeItem('username');
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          this.currentUserSource.next(null);
          this.activeUser.next(false);
        })
      );
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

  getUsername() {
    let username;
    if (this.isUserActive()) username = localStorage.getItem('username');
    return username;
  }

  isUserActive(): boolean {
    if (this.activeUser.asObservable()) return true;
    return false;
  }
}
