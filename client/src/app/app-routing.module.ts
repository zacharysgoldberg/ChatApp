import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { ChatsComponent } from './chats/chats.component';
import { AuthGuard } from './_guards/auth.guard';
import { NavbarComponent } from './navbar/navbar.component';
import { NotificationsComponent } from './notifications/notifications.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      { path: 'account', component: NavbarComponent },
      { path: 'members', component: MemberListComponent },
      { path: 'members/:id', component: MemberDetailComponent },
      { path: 'chats', component: ChatsComponent },
      { path: 'notifications', component: NotificationsComponent },
      { path: '**', component: HomeComponent, pathMatch: 'full' },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
