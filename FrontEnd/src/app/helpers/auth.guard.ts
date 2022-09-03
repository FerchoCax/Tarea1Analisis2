import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, CanDeactivate, CanLoad, Route, UrlSegment, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthenticationService } from '../services/authentication/authentication.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private router: Router,
    private authenticationService: AuthenticationService
  ) { }
  
  private tokenExpired(token: string) {
    const expiry = (JSON.parse(atob(token.split('.')[1]))).exp;
    return (Math.floor((new Date).getTime() / 1000)) >= expiry;
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    const user = this.authenticationService.userValue;

    if (user) {
      if (this.tokenExpired(user.token)) {
        // role not authorised so redirect to home page
        this.authenticationService.logout();
        return false;
      }

      // check if route is restricted by role
      let encontrado: boolean = false;
      
      if (user != undefined && route.data.roles != undefined) {
        user.roles.forEach((element: string) => {
          route.data.roles.forEach((Ele: string) => {
            if (element == Ele) {
              encontrado = true;
            }
          });
        });
      }
      
      if (route.data.roles && !encontrado) {
        // role not authorised so redirect to home page
        this.router.navigate(['/']);
        return false;
      }
      
      // authorised so return true
      return true;
    }

    // not logged in so redirect to login page with the return url
    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });

    return false;
  }
}
