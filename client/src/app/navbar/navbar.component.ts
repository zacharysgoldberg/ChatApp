// sidebar.component.ts
import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent implements OnInit {
  @Output() navigateSidebar = new EventEmitter<string>();
  @ViewChild('sidebar') sidebar: ElementRef | undefined;

  isDropup = true;
  isContactsActive: boolean = false;
  isMessagesActive: boolean = false;
  isSidebarCollapsed: boolean = false;

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

  toggleContacts(): void {
    this.isContactsActive = !this.isContactsActive;
  }

  toggleMessages() {
    this.isMessagesActive = !this.isMessagesActive;
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}
