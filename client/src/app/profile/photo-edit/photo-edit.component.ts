import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FileUploadModule, FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs';
import { MemberModel } from 'src/app/_models/member.model';
import { UserModel } from 'src/app/_models/user.model';
import { AccountService } from 'src/app/_services/account.service';
import { UserService } from 'src/app/_services/user.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-edit',
  templateUrl: './photo-edit.component.html',
  styleUrls: ['./photo-edit.component.css'],
})
export class PhotoEditComponent implements OnInit {
  @Input() member: MemberModel | undefined;
  user: UserModel | undefined;
  uploader: FileUploader | undefined;
  baseUrl = environment.apiUrl;
  hasBaseDropZoneOver = false;
  @Output() cancelEdit = new EventEmitter();

  constructor(
    private accountService: AccountService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.user = user;
          this.initializeUploader();
        }
      },
    });
  }

  cancel() {
    this.cancelEdit.emit(false);
  }

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
      location.reload();
    };
  }

  async updatePhoto() {
    // Ensure the user is authenticated before making a new request
    if (!this.user) {
      this.cancel();
      return;
    }
    this.user = await this.accountService.getAuthenticatedUser(this.user);

    if (!this.uploader) return;
    this.uploader.authToken = 'Bearer ' + this.user.accessToken;

    if (this.member?.photoUrl) {
      this.userService.deletePhoto().subscribe({
        next: () => {
          console.log('Previous photo deleted successfully.');
          // Upload new photo after the previous one is deleted
          this.uploader?.uploadAll();
        },

        error: (error) =>
          console.error('Failed to delete previous photo:', error),
      });
    } else {
      // If no previous photo, directly upload new photo
      this.uploader?.uploadAll();
    }
  }

  deletePhoto() {
    this.userService.deletePhoto().subscribe({
      next: (response) => {
        console.log(response);
        this.cancel();
        location.reload();
      },

      error: (error) => console.log(error),
    });
  }
}
