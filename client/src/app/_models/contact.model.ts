import { Photo } from './photo.model';

export interface ContactModel {
  id: number;
  userName: string;
  email: string;
  // created: Date;
  lastActive: Date;
  photoUrl?: string;
}
