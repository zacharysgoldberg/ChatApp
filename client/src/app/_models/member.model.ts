import { ContactModel } from './contact.model';
import { Photo } from './photo.model';

export interface MemberModel {
  id: number;
  userName: string;
  email: string;
  refreshToken: string;
  refreshTokenExpiryTime: string;
  created: Date;
  lastActive: Date;
  photoUrl: string;
  contacts: ContactModel[];
}
