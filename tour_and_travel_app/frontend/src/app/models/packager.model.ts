export interface PublicPackagerResponse {
  id: string;
  companyName: string;
  description: string;
  websiteUrl: string;
  contactEmail: string;
  avgRating: number;
  totalReviews: number;
  totalPackagesContributed: number;
}

export interface PackagerReviewResponse {
  id: string;
  packageId: string;
  packageName: string;
  reviewerId: string;
  reviewerName: string;
  overallRating: number;
  comment: string;
  createdAt: string;
}
