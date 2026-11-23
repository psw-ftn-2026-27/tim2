export interface WalletBalanceDto {
  balance: number;
}

export interface TransactionDto {
  id: string;
  amount: number;
  createdAt: string;
  isCancelled: boolean;
  name: string | null;
}

export interface UserWalletDto {
  userId: string;
  email: string;
  balance: number;
}

export interface DepositDto {
  amount: number;
}

export interface UserTransactionDto {
  id: string;
  amount: number;
  createdAt: string;
  adminId: string | null;
  isCancelled: boolean;
  name: string | null;
}

export interface TourOfferDto {
  tourId: string;
  name: string;
  price: number;
}

export interface CartItemDto {
  tourId: string;
  name: string;
  price: number;
}

export interface CartDto {
  items: CartItemDto[];
  totalPrice: number;
}

export interface PurchasedTourDto {
  tourId: string;
  name: string;
  price: number;
  purchasedAt: string;
}

export interface PurchaseGiftCardDto {
  recipientEmail: string;
  amount: number;
  message: string | null;
}

export type GiftCardStatus = 'Valid' | 'Used' | 'Expired';

export interface GiftCardDto {
  id: string;
  amount: number;
  message: string;
  otherPartyEmail: string;
  expiresAt: string;
  status: GiftCardStatus;
}

export interface GiftCardOverviewDto {
  received: GiftCardDto[];
  sent: GiftCardDto[];
}

export interface CreateCouponDto {
  productId: string | null;
  percent: number;
  expiresOn: string;
}

export interface CouponDto {
  id: string;
  code: string;
  percent: number;
  expiresOn: string;
  productId: string | null;
  productName: string | null;
}

export interface CouponProductDto {
  id: string;
  name: string;
}
