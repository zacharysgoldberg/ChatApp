import { Photo } from './photo.model';

export interface Contact {
  id: number;
  userName: string;
  email: string;
  lastActive: Date;
  photoUrl: string;
}
