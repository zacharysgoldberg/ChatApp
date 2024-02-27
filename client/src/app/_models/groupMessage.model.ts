import { ContactModel } from './contact.model';

export interface GroupMessageModel {
  id: number;
  channelId: string;
  channelName: string;
  senderId: number;
  senderUsername: string;
  senderPhotoUrl?: string;
  content: string;
  createdAt: Date;
  contacts: ContactModel[];
}
