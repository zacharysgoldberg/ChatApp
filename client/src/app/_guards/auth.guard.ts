import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';

export const AuthGuard: CanActivateFn = async (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  const router = inject(Router);
  const jwtHelper = inject(JwtHelperService);

  const token = localStorage.getItem('accessToken');

  if (token && !jwtHelper.isTokenExpired(token)) {
    // console.log(jwtHelper.decodeToken(token));
    return true;
  }

  const isRefreshSuccess = await accountService.tryRefreshingTokens(token);
  if (!isRefreshSuccess) {
    router.navigateByUrl('/');
    toastr.error('Must be signed in');
  }

  return isRefreshSuccess;

  // return accountService.currentUser$.pipe(
  //   map((user) => {
  //     if (user) return true;
  //     else {
  //       toastr.error('You shall not pass!');
  //       return false;
  //     }
  //   })
  // );
};
