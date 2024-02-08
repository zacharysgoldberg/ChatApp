import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { NavbarComponent } from './navbar/navbar.component';
import { NotificationsComponent } from './notifications/notifications.component';
import { SharedModule } from './_modules/shared.module';
import { TestErrorComponent } from './errors/test-error/test-error.component';
import { ErrorInterceptor } from './_interceptors/error.interceptor';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { JwtInterceptor, JwtModule } from '@auth0/angular-jwt';
import { AuthGuard } from './_guards/auth.guard';
import { ContactsComponent } from './contacts/contacts.component';
import { environment } from 'src/environments/environment.development';
import { ChatComponent } from './chat/chat.component';
import { LoginComponent } from './login/login.component';
import { ProfileDetailsComponent } from './profile/profile-details/profile-details.component';
import { ProfileEditComponent } from './profile/profile-edit/profile-edit.component';

export function tokenGetter() {
  return localStorage.getItem('accessToken');
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    RegisterComponent,
    NavbarComponent,
    NotificationsComponent,
    TestErrorComponent,
    NotFoundComponent,
    ServerErrorComponent,
    ContactsComponent,
    ChatComponent,
    LoginComponent,
    ProfileDetailsComponent,
    ProfileEditComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
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
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
