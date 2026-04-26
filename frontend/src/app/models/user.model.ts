export interface User {
  id: number;
  name: string;
  email: string;
  mobileNumber: string;
  age: number;
  gender: string;
  role: 'User' | 'Operator' | 'Admin';
  isApproved: boolean;
  createdAt: string;
}
