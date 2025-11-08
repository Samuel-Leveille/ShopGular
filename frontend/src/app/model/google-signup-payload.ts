export interface GoogleSignupPayload {
  pendingToken: string;
  accountType: 'client' | 'seller';
  firstName?: string;
  lastName?: string;
  sellerName?: string;
}

