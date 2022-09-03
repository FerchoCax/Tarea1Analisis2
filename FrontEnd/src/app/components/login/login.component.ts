import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { first } from 'rxjs/operators';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
    loading = false;
    submitted = false;
    error = '';
    hide = true;
    visible=true;
    constructor(
        private formBuilder: FormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private authenticationService: AuthenticationService,
        public dialog: MatDialog
    ) {
        // redirect to home if already logged in
        if (this.authenticationService.userValue) {
            this.router.navigate(['/']);
        }
    }
    cambio(){
        if (this.hide==true){
            this.hide= false;
        }else if(this.hide == false){
            this.hide = true;
        }
    }
    cambiocontrasena(post:any){
        let dato:String = this.f.password.value
        
        if(dato.length>0){

            this.visible=false
        }else if(dato==''){

            this.visible == true;
        }
    }
   
    ngOnInit() {
        this.loginForm = this.formBuilder.group({
            username: ['', Validators.required],
            password: ['', Validators.required]
        });
    }

    // convenience getter for easy access to form fields
    get f() { return this.loginForm.controls; }

    onSubmit() {
        this.submitted = true;


        
        
        // stop here if form is invalid
        if (this.loginForm.invalid) {
            return;
        }

        this.loading = true; 
        this.authenticationService.login(this.f.username.value, this.f.password.value)
            .pipe(first())
            .subscribe({
                next: () => {
                    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
                    this.router.navigateByUrl(returnUrl);
                },
                error: error => {
                    if (error.status == 401){
                        this.error = "Credenciales incorrectas"
                    }else if(error.status == 500) {
                        this.error = "Ocurrió un error en el servidor";
                    }
                    else if(error.status == 400)   {
                        this.error = error.error.message;
                    }
                    else {
                        this.error = "Ocurrió un error inesperado";
                    }
                    //   this.error = error;
                    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
                    this.router.navigateByUrl(returnUrl);
                    this.loading = false;
                }
            });
    }
}