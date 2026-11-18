import { Routes } from '@angular/router';
import { BuyGiftCard } from './gift-card-gifting/buy-gift-card/buy-gift-card';
import { GiftCards } from './gift-card-gifting/gift-cards/gift-cards';
import { PaymentHome } from './payment-home/payment-home';
import { PurchasedTours } from './purchase-viewing/purchased-tours/purchased-tours';
import { AddToCart } from './shopping/add-to-cart/add-to-cart';
import { Cart } from './shopping/cart/cart';
import { UserWallet } from './wallet-funding/user-wallet/user-wallet';
import { UserWallets } from './wallet-funding/user-wallets/user-wallets';
import { Wallet } from './wallet-viewing/wallet/wallet';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: Wallet },
  { path: 'wallets', component: UserWallets },
  { path: 'wallets/:userId', component: UserWallet },
  { path: 'cart', component: Cart },
  { path: 'cart/add/:tourId', component: AddToCart },
  { path: 'purchases', component: PurchasedTours },
  { path: 'gift-cards', component: GiftCards },
  { path: 'gift-cards/buy', component: BuyGiftCard },
];
