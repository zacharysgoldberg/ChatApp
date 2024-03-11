import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { NotificationModel } from '../_models/notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  apiUrl = environment.apiUrl;

  notifications: NotificationModel[] = [];

  constructor(private http: HttpClient) {}

  getNotification(notificationId: number) {
    return this.http.get<NotificationModel>(
      this.apiUrl + `notifications/${notificationId}`
    );
  }

  getUserNotifications() {
    if (this.notifications.length > 0) return of(this.notifications);

    return this.http
      .get<NotificationModel[]>(this.apiUrl + 'notifications')
      .pipe(
        map((notifications) => {
          this.notifications = notifications;
          return this.notifications;
        })
      );
  }

  deleteNotification(notificationId: number) {
    this.notifications = this.notifications.filter(
      (notification) => notification.id !== notificationId
    );

    return this.http.delete<void>(
      this.apiUrl + `notifications/${notificationId}`
    );
  }
}
