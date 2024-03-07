import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { UserModel } from '../_models/user.model';
import { BehaviorSubject, take } from 'rxjs';
import { Router } from '@angular/router';
import { MessageService } from './message.service';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private onlineUsersSource = new BehaviorSubject<number[]>([]);
  onlineUser$ = this.onlineUsersSource.asObservable();

  constructor(
    private toastr: ToastrService,
    private router: Router,
    private messageService: MessageService
  ) {}

  createHubConnection(user: UserModel) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.accessToken,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch((error) => console.log(error));

    this.hubConnection.on('UserIsOnline', (userId) => {
      this.onlineUser$.pipe(take(1)).subscribe({
        next: (userIds) => {
          const updatedUserIds = [...userIds, userId]; // Add user to the list
          this.onlineUsersSource.next(updatedUserIds);
        },
      });
    });

    this.hubConnection.on('UserIsOffline', (userId) => {
      this.onlineUser$.pipe(take(1)).subscribe({
        next: (userIds) => {
          const updatedUserIds = userIds.filter((id) => id !== userId); // Remove user from the list
          this.onlineUsersSource.next(updatedUserIds);
        },
      });
    });

    this.hubConnection.on('GetOnlineUsers', (userIds) => {
      this.onlineUsersSource.next(userIds);
    });

    this.hubConnection.on('NewMessageReceived', ({ id, username }) => {
      this.toastr
        .info(username + ' has sent you a new message.')
        .onTap.pipe(take(1))
        .subscribe({
          next: () => {
            this.messageService.setContactForMessageThread(id);
            this.router.navigate(['/chat']);
          },
        });
    });
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch((error) => console.log(error));
  }
}
