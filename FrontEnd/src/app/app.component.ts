import { Component, HostListener } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'Frontend';
  theme: boolean = false;

  onActivate(event) {
    window.scroll(0,0);
  }

  ngOnInit() {
    if(localStorage.getItem("theme") == null){
      localStorage.setItem("theme","light");
    }
    let theme: string = localStorage.getItem("theme");
    if(theme == 'light'){      
      (<HTMLBodyElement><unknown>document.getElementById("rooot")).classList.remove("dark");
      (<HTMLBodyElement><unknown>document.getElementById("rooot")).classList.add("light");
    }else{      
      (<HTMLBodyElement><unknown>document.getElementById("rooot")).classList.remove("light");
      (<HTMLBodyElement><unknown>document.getElementById("rooot")).classList.add("dark");
    }
  }
  
  ngOnDestroy(): void{
    localStorage.setItem('user', null);
  }
}
