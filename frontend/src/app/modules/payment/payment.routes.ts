import { Routes } from '@angular/router';
import { PaymentHome } from './payment-home/payment-home';
import { Wallet } from './wallet-viewing/wallet/wallet';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: Wallet },
];
