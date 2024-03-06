import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { MemberModel } from '../../_models/member.model';
import { UserModel } from '../../_models/user.model';
import { AccountService } from '../../_services/account.service';
import { take } from 'rxjs';
import { UserService } from 'src/app/_services/user.service';
import { PhotoEditComponent } from '../photo-edit/photo-edit.component';

@Component({
  selector: 'app-profile-details',
  templateUrl: './profile-details.component.html',
  styleUrls: ['./profile-details.component.css'],
})
export class ProfileDetailsComponent implements OnInit {
  user?: UserModel;
  member: MemberModel | undefined;
  editPhotoMode: boolean = false;
  editProfileMode: boolean = false;
  editField: string | undefined;

  constructor(
    private accountService: AccountService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) {
          this.user = user;
          this.loadProfile();
        }
      },
    });
  }

  loadProfile() {
    const username = this.accountService.getUsername();
    if (!username) return;

    this.userService.getMember(username).subscribe({
      next: (member) => {
        this.member = member;
      },
    });
  }

  editPhotoToggle() {
    this.editPhotoMode = !this.editPhotoMode;
  }

  cancelEditPhotoMode(event: boolean) {
    this.editPhotoMode = event;
  }

  editProfileModeToggle(field: string) {
    this.editProfileMode = !this.editProfileMode;
    this.editField = field;
  }

  cancelEditProfileMode(event: boolean) {
    this.editProfileMode = event;
  }

  // Closes the modal when backdrop is clicked
  closeModalOnBackdropClick(event: MouseEvent) {
    // Check if the click event target is the backdrop element
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.editPhotoMode = false;
    }
  }
}
