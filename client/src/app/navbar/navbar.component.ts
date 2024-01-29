// sidebar.component.ts
import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  ViewChild,
  ElementRef,
} from '@angular/core';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent implements OnInit {
  @Output() navigateSidebar = new EventEmitter<string>();
  @ViewChild('sidebar') sidebar: ElementRef | undefined;

  isDropup = true;

  model: any = {};

  constructor(
    public accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {}

  logout() {
    this.accountService.logout(this.model).subscribe({
      next: () => {
        // console.log(response);
        this.router.navigateByUrl('/');
      },

      error: (error) => {
        this.toastr.error(error.error), console.log(error);
      },
    });
  }

  navigateToProfile(): void {
    // Implement navigation logic to the profile page
    this.router.navigate(['/profile']);
  }
}
