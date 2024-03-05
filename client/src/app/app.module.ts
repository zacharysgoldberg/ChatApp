import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { NavbarComponent } from './navbar/navbar.component';
import { SharedModule } from './_modules/shared.module';
import { TestErrorComponent } from './errors/test-error/test-error.component';
import { ErrorInterceptor } from './_interceptors/error.interceptor';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { JwtInterceptor, JwtModule } from '@auth0/angular-jwt';
import { AuthGuard } from './_guards/auth.guard';
import { ContactsComponent } from './contacts/contacts.component';
import { environment } from 'src/environments/environment.development';
import { ChatListComponent } from './chat/chat-list-contacts/chat-list.component';
import { LoginComponent } from './login/login.component';
import { ProfileDetailsComponent } from './profile/profile-details/profile-details.component';
import { ProfileEditComponent } from './profile/profile-edit/profile-edit.component';
import { LoadingInterceptor } from './_interceptors/loading.interceptor';
import { PhotoEditComponent } from './profile/photo-edit/photo-edit.component';
import { UserModel } from './_models/user.model';
import { TextInputComponent } from './_forms/text-input/text-input.component';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';
import { ChatMessageThreadComponent } from './chat/chat-message-thread/chat-message-thread.component';
import { ChatCreateChannelComponent } from './chat/chat-create-channel/chat-create-channel.component';
import { ChatGroupMessageChannelComponent } from './chat/chat-group-message-channel/chat-group-message-channel.component';

export function tokenGetter() {
  const userString = localStorage.getItem('user');

  if (!userString) return null;

  const user: UserModel = JSON.parse(userString);

  return user.accessToken;
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    RegisterComponent,
    NavbarComponent,
    TestErrorComponent,
    NotFoundComponent,
    ServerErrorComponent,
    ContactsComponent,
    ChatListComponent,
    ChatMessageThreadComponent,
    LoginComponent,
    ProfileDetailsComponent,
    ProfileEditComponent,
    PhotoEditComponent,
    TextInputComponent,
    AdminPanelComponent,
    ChatCreateChannelComponent,
    ChatGroupMessageChannelComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    SharedModule,
    JwtModule.forRoot({
      config: {
        tokenGetter: tokenGetter,
        allowedDomains: [environment.allowedDomains],
        disallowedRoutes: [],
      },
    }),
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: LoadingInterceptor, multi: true },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
