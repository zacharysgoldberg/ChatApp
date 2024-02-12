import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AuthGuard } from './_guards/auth.guard';
import { NotificationsComponent } from './notifications/notifications.component';
import { TestErrorComponent } from './errors/test-error/test-error.component';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { ContactsComponent } from './contacts/contacts.component';
import { ChatComponent } from './chat/chat.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ProfileDetailsComponent } from './profile/profile-details/profile-details.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      { path: 'home', component: HomeComponent },
      { path: 'profile', component: ProfileDetailsComponent },
      { path: 'contacts', component: ContactsComponent },
      { path: 'chats', component: ChatComponent },
      { path: 'notifications', component: NotificationsComponent },
    ],
  },
  { path: 'errors', component: TestErrorComponent },
  { path: 'not-found', component: NotFoundComponent },
  { path: 'server-error', component: ServerErrorComponent },
  { path: '**', component: HomeComponent, pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
