import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { UserModel } from '../_models/user.model';

export const AuthGuard: CanActivateFn = async (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  const router = inject(Router);
  const jwtHelper = inject(JwtHelperService);

  const userString = localStorage.getItem('user');

  if (!userString) return false;

  const user: UserModel = JSON.parse(userString);
  const token = user.accessToken;

  if (token && !jwtHelper.isTokenExpired(token)) {
    // console.log(jwtHelper.decodeToken(token));
    return true;
  }

  const isRefreshSuccess = await accountService.tryRefreshingTokens(token);
  if (!isRefreshSuccess) {
    router.navigateByUrl('/');
    // toastr.error('Must be signed in');
  }

  return isRefreshSuccess;
};
