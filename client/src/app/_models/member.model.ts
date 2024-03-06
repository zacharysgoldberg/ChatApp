import { ContactModel } from './contact.model';
import { Photo } from './photo.model';

export interface MemberModel {
  id: number;
  userName: string;
  email: string;
  refreshToken: string;
  refreshTokenExpiryTime: string;
  memberSince: Date;
  lastActive: Date;
  photoUrl: string;
  phoneNumber: string;
  contacts: ContactModel[];
}
