
import {Injectable} from '@angular/core';
import {BehaviorSubject} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SharedDataService {

  public shared$ = new BehaviorSubject({
    value: {},
    type: undefined,
    additional: null
  });

  constructor() {
  }
  
  public shareData(data: any): void {
    this.shared$.next(data);
  }
}
