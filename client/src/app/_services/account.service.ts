import { HttpClient, HttpHeaders } from '@angular/common/http';
import { LoginModel } from '../_models/login.model';
import { UserModel } from '../_models/user.model';
import { RegisterModel } from '../_models/register.model';
import {
  BehaviorSubject,
  Observable,
  Subject,
  firstValueFrom,
  map,
} from 'rxjs';
import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { ResetPasswordModel } from '../_models/resetPassword.model';
import { PresenceService } from './presence.service';
import { ForgotPasswordModel } from '../_models/forgotPassword.model';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  apiUrl = environment.apiUrl;

  private currentUserSource = new BehaviorSubject<UserModel | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(
    private http: HttpClient,
    private jwtHelper: JwtHelperService,
    private presenceService: PresenceService
  ) {
    const userString = localStorage.getItem('user');

    if (userString) {
      const user: UserModel = JSON.parse(userString);
      this.setCurrentUserSource(user);
      this.currentUser$ = this.currentUserSource.asObservable();
    }
  }

  login(credentials: LoginModel) {
    return this.http
      .post<UserModel>(this.apiUrl + 'account/login', credentials)
      .pipe(
        map((response: UserModel) => {
          const user = response;
          if (user) this.setCurrentUserSource(user);
        })
      );
  }

  register(registration: RegisterModel) {
    return this.http
      .post<UserModel>(this.apiUrl + 'account/register', registration)
      .pipe(
        map((user) => {
          if (user) this.setCurrentUserSource(user);
        })
      );
  }

  logout() {
    // const username = this.getUsername();
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presenceService.stopHubConnection();

    // return this.http.post(this.apiUrl + `account/revoke/${username}`, model);
  }

  forgotPassword(forgotPasswordModel: ForgotPasswordModel) {
    return this.http.post<{ token: string }>(
      this.apiUrl + 'account/forgot-password',
      forgotPasswordModel
    );
  }

  resetPassword(resetPassword: ResetPasswordModel) {
    return this.http.post<any>(
      this.apiUrl + 'account/reset-password',
      resetPassword
    );
  }

  setCurrentUserSource(user: UserModel) {
    user.roles = [];
    const roles = this.getDecodedToken(user.accessToken).role;
    if (roles) {
      Array.isArray(roles) ? (user.roles = roles) : user.roles.push(roles);
      this.setUserAccess(user);
      this.currentUserSource.next(user);
      this.presenceService.createHubConnection(user);
    }
  }

  setUserAccess(user: UserModel): void {
    localStorage.setItem('user', JSON.stringify(user));
  }

  getDecodedToken(token: string) {
    const tokenParts = token.split('.');
    if (tokenParts.length !== 3) {
      console.error('Token does not have expected format');
      return null;
    }
    return JSON.parse(atob(tokenParts[1]));
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

    try {
      const refreshRes = await firstValueFrom(
        this.http.post<UserModel>(
          this.apiUrl + 'account/refresh-token',
          credentials,
          {
            headers: new HttpHeaders({
              'Content-Type': 'application/json',
            }),
          }
        )
      );

      user.accessToken = refreshRes.accessToken;
      user.refreshToken = refreshRes.refreshToken;
      localStorage.setItem('user', JSON.stringify(user));

      return true; // Refresh succeeded
    } catch (error) {
      console.error('Token refresh failed:', error);
      return false; // Refresh failed
    }
  }

  async getAuthenticatedUser(currentUser: UserModel): Promise<UserModel> {
    if (
      !this.isUserAuthenticated() ||
      this.jwtHelper.isTokenExpired(currentUser.accessToken)
    ) {
      const isRefreshSuccess = await this.tryRefreshingTokens(
        currentUser.accessToken
      );
      if (!isRefreshSuccess) return currentUser;

      const userString = localStorage.getItem('user');

      if (!userString) return currentUser;

      const newUser: UserModel = JSON.parse(userString!);
      this.setCurrentUserSource(newUser);
      return newUser;
    }

    return currentUser;
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
