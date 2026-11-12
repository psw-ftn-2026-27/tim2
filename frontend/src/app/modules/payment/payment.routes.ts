import { Routes } from '@angular/router';
import { PaymentHome } from './payment-home/payment-home';
import { UserWallet } from './wallet-funding/user-wallet/user-wallet';
import { UserWallets } from './wallet-funding/user-wallets/user-wallets';
import { Wallet } from './wallet-viewing/wallet/wallet';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: Wallet },
  { path: 'wallets', component: UserWallets },
  { path: 'wallets/:userId', component: UserWallet },
];
