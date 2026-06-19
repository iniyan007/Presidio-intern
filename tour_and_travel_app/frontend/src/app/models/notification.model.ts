export interface AppNotification {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: number | string;
  referenceId?: string;
  isRead: boolean;
  createdAt: string;
}
