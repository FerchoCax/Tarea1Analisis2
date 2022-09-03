import { BreakpointObserver } from '@angular/cdk/layout';
import { ChangeDetectorRef, Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { UserModel } from 'src/app/models/UserModel';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { CentrosProduccion } from 'src/app/models/CentrosProduccion';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-home-layout',
  templateUrl: './home-layout.component.html',
  styleUrls: ['./home-layout.component.scss']
})
export class HomeLayoutComponent implements OnInit {

  isExpanded: boolean = true;
  user: UserModel;
  smallView: boolean = false;
  @ViewChild(MatSidenav)
  sidenav!: MatSidenav;
  isLoggedIn$: Observable<boolean>;
  isDarkMode: boolean = false;
  centro: CentrosProduccion = {
    nombreCentro: 'Sin centro',
    codCentro: '00'
  };

  constructor(private authenticationService: AuthenticationService,
    private observer: BreakpointObserver,
    private router: Router,
    private cd: ChangeDetectorRef) {

    this.authenticationService.user.subscribe(x => {
      this.user = x;
      this.isLoggedIn$ = this.authenticationService.isLoggedIn;
    });
  }

  onActivate(event) {
    window.scroll(0, 0);
  }

  ruta() {
    if (this.router.url == '/') {
      return true;
    }
    return false;
  }

  toggleNav() {
    if (!this.smallView) {
      this.isExpanded = !this.isExpanded;

      return;
    }
    if (this.sidenav.opened) {
      this.sidenav.close();
    } else {
      this.sidenav.open();
    }
  }

  ngOnInit() {
    

    let theme: string = localStorage.getItem("theme");

    if (theme == 'dark') {
      this.isDarkMode = true;
    }

    this.isLoggedIn$ = this.authenticationService.isLoggedIn; // {2}
  }

  ngOnDestroy() {
  }

  darkTheme() {
    let theme: string = localStorage.getItem("theme");
    if (theme == 'dark') {
      (<HTMLBodyElement><unknown>document.getElementById("rooot")).classList.remove("dark");
      (<HTMLBodyElement><unknown>document.getElementById("rooot")).classList.add("light");
      localStorage.setItem("theme", "light");
      this.isDarkMode = false;
    } else {
      (<HTMLBodyElement><unknown>document.getElementById("rooot")).classList.remove("light");
      (<HTMLBodyElement><unknown>document.getElementById("rooot")).classList.add("dark");
      localStorage.setItem("theme", "dark");
      this.isDarkMode = true;
    }
  }

  public GoTo(ruta: string): void {
    if (this.smallView && this.sidenav.opened) {
      this.sidenav.close();
    }
    this.router.navigate([ruta]);
  }

  logout() {
    Swal.fire({
      title: '¿Desea salir del sistema?',
      icon: 'question',
      confirmButtonText: 'Aceptar',
      showCancelButton: true,
      cancelButtonText: 'Cancelar'
    }).then(result => {
      if (result.isConfirmed) {
        this.authenticationService.logout();
      }
    })
  }

  TieneRol(rol: string[]): boolean {
    return this.authenticationService.validarRol(rol)
  }
}