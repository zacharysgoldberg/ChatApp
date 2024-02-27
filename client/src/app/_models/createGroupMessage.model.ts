export interface CreateGroupMessageModel {
  channelId?: string;
  channelName: string;
  content?: string;
  contactIds: number[];
}
