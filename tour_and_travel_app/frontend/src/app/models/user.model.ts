export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  profilePicture: string | null;
  isActive: boolean;
  isEmailVerified: boolean;
  isPackager: boolean;
}

export interface UpdateProfileRequest {
  fullName: string;
  phone?: string | null;
}
