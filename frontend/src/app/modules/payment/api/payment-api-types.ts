export interface WalletBalanceDto {
  balance: number;
}

export interface TransactionDto {
  id: string;
  amount: number;
  createdAt: string;
  isCancelled: boolean;
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
  adminId: string;
  isCancelled: boolean;
}
