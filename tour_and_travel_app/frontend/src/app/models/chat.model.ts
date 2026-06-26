export interface MessageResponse {
  id: string;
  threadId: string;
  senderId: string;
  senderName: string;
  body: string;
  isRead: boolean;
  sentAt: string;
}

export interface MessageThreadResponse {
  id: string;
  userId: string;
  userName: string;
  packagerId: string;
  packagerName: string;
  packageId?: string;
  packageTitle?: string;
  createdAt: string;
  lastMessageAt?: string;
  unreadCount: number;
}

export interface SendMessageRequest {
  threadId: string;
  senderRole: string | number;
  body: string;
}

export interface CreateThreadRequest {
  packagerId: string;
  packageId?: string;
}
