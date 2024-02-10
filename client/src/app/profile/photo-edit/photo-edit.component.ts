import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { FileUploadModule, FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs';
import { MemberModel } from 'src/app/_models/member.model';
import { UserModel } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-edit',
  templateUrl: './photo-edit.component.html',
  styleUrls: ['./photo-edit.component.css'],
})
export class PhotoEditComponent implements OnInit {
  @Output() cancelEdit = new EventEmitter();
  @Input() member: MemberModel | undefined;
  user: UserModel | undefined;
  uploader: FileUploader | undefined;

  baseUrl = environment.apiUrl;
  hasBaseDropZoneOver = false;

  constructor(private accountService: AccountService, private router: Router) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });

    this.initializeUploader();
  }

  cancel() {
    this.cancelEdit.emit(false);
  }

  async updatePhoto() {
    if (!this.user) return;

    this.user.accessToken = await this.accountService.getAccessToken(this.user);

    if (!this.uploader) return;

    this.uploader.authToken = 'Bearer ' + this.user.accessToken;

    this.uploader?.uploadAll();
  }

  // Delete user's photo
  deletePhoto() {}

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/update-photo',
      authToken: 'Bearer ' + this.user?.accessToken,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo = JSON.parse(response);
        console.log(photo);
        if (this.member) {
          this.member.photoUrl = photo.url;
        }
      }
      this.cancel();
    };
  }
}
