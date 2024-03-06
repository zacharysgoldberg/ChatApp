import { AccountService } from '../_services/account.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { NotificationModel } from '../_models/notification.model';
import { Observable, map, of, switchMap, take } from 'rxjs';
import { MemberModel } from '../_models/member.model';
import { UserService } from '../_services/user.service';
import { UserModel } from '../_models/user.model';
import { NotificationService } from '../_services/notification.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent implements OnInit {
  user?: UserModel;
  member: MemberModel | undefined;
  notifications$: Observable<NotificationModel[]> = of([]);
  isDropup = true;
  isAdmin = false;
  @Output() notificationLoaded: EventEmitter<number | string> =
    new EventEmitter<number | string>();

  constructor(
    public accountService: AccountService,
    private userService: UserService,
    private notificationService: NotificationService,
    private messageService: MessageService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });

    this.notifications$ = this.notificationService.getUserNotifications();

    const username = this.accountService.getUsername();

    if (!username) return;

    this.userService.getMember(username).subscribe({
      next: (member) => {
        this.member = member;
      },
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  navigateToProfile() {
    this.router.navigateByUrl('/profile');
  }

  async loadNotification(notificationId: number) {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (this.notifications$) {
      this.notifications$
        .pipe(
          map((notifications) =>
            notifications.find(
              (notification) => notification.id === notificationId
            )
          )
        )
        .subscribe(async (notification) => {
          if (notification) {
            if (notification.channelId) {
              this.notificationLoaded.emit(notification.channelId);
            } else {
              const senderId = notification.senderId;
              await this.messageService.setContactForMessageThread(senderId);
              this.notificationLoaded.emit(senderId);
            }
            this.router.navigate(['../chat']);
          }
        });
    }
  }

  async deleteNotification(notification: NotificationModel) {
    if (!this.user) return;
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    this.notificationService.deleteNotification(notification.id).subscribe({
      next: () => {
        // Remove the deleted notification from the list of notifications
        this.notifications$ = this.notifications$.pipe(
          map((notifications) =>
            notifications.filter((n) => n.id !== notification.id)
          )
        );
        this.toastr.success('Notification deleted successfully.');
      },
      error: (error) => {
        console.error('Error deleting notification:', error);
        this.toastr.error('Failed to delete notification.');
      },
    });
  }
}
