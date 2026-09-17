import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/pages/login.component';
import { AdminDashboardComponent } from './features/admin/dashboard/admin-dashboard.component';
import { ClientDashboardComponent } from './features/client/dashboard/client-dashboard.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'admin/dashboard', component: AdminDashboardComponent },
  { path: 'client/dashboard', component: ClientDashboardComponent },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' }
];
