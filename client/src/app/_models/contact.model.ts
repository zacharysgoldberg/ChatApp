import { Photo } from './photo.model';

export interface ContactModel {
  id: number;
  userName: string;
  email: string;
  lastActive: Date;
  photoUrl: string;
}
