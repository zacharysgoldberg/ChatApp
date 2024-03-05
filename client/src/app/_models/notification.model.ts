export interface NotificationModel {
  id: number;
  senderId: number;
  senderUsername: string;
  recipientId?: number;
  recipientUsername?: string;
  messageId?: number;
  groupMessageId?: number;
  channelId?: string;
  createdAt: Date;
  content: string;
}
