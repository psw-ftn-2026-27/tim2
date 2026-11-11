export interface WalletBalanceDto {
  balance: number;
}

export interface TransactionDto {
  id: string;
  amount: number;
  createdAt: string;
  isCancelled: boolean;
}
