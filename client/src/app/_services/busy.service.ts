import { Injectable, OnInit } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root',
})
export class BusyService implements OnInit {
  busyRequestCount = 0;

  constructor(private spinnerService: NgxSpinnerService) {}

  ngOnInit(): void {}

  busy() {
    this.busyRequestCount++;
    this.spinnerService.show(undefined, {
      type: 'line-scale-pulse-out',
      bdColor: 'rgbd(255,255,255,0)',
      color: '#fffffff',
    });
  }

  idle() {
    this.busyRequestCount--;
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide();
    }
  }
}
