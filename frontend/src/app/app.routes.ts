import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/guards/auth.guard';
import { roleGuard } from './core/auth/guards/role.guard';
import { LoginComponent } from './features/auth/pages/login.component';
import { RegisterComponent } from './features/auth/pages/register.component';
import { AdminDashboardComponent } from './features/admin/dashboard/admin-dashboard.component';
import { ClientDashboardComponent } from './features/client/dashboard/client-dashboard.component';
import { CreditSimulatorComponent } from './features/credits/simulator/credit-simulator.component';
import { CreditComparisonComponent } from './features/credits/comparison/credit-comparison.component';
import { InvestmentSimulatorComponent } from './features/investments/simulator/investment-simulator.component';
import { InvestmentApplicationComponent } from './features/investments/application/investment-application.component';
import { InstitutionComponent } from './features/admin/institution/institution.component';
import { CreditTypesComponent } from './features/admin/credits/credit-types/credit-types.component';
import { CreditRatesComponent } from './features/admin/credits/credit-rates/credit-rates.component';
import { CreditChargesComponent } from './features/admin/credits/credit-charges/credit-charges.component';
import { InvestmentProductsComponent } from './features/admin/investments/investment-products/investment-products.component';
import { InvestmentRatesComponent } from './features/admin/investments/investment-rates/investment-rates.component';
import { InvestmentApplicationsComponent } from './features/admin/investments/applications/investment-applications.component';
import { AdvisorDashboardComponent } from './features/advisor/advisor-dashboard.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { ClientLayoutComponent } from './layouts/client-layout/client-layout.component';

export const routes: Routes = [
  { path: 'login', component: AuthLayoutComponent, canActivate: [guestGuard], children: [{ path: '', component: LoginComponent }] },
  { path: 'register', component: AuthLayoutComponent, canActivate: [guestGuard], children: [{ path: '', component: RegisterComponent }] },
  { path: 'admin', component: AdminLayoutComponent, canActivate: [authGuard, roleGuard(['Admin'])], children: [
    { path: 'dashboard', component: AdminDashboardComponent }, { path: 'institution', component: InstitutionComponent },
    { path: 'credits/types', component: CreditTypesComponent }, { path: 'credits/rates', component: CreditRatesComponent }, { path: 'credits/charges', component: CreditChargesComponent }, { path: 'credits/simulator', component: CreditSimulatorComponent }, { path: 'credits/comparison', component: CreditComparisonComponent },
    { path: 'investments/products', component: InvestmentProductsComponent }, { path: 'investments/rates', component: InvestmentRatesComponent }, { path: 'investments/applications', component: InvestmentApplicationsComponent }, { path: 'investments/applications/:id', component: InvestmentApplicationsComponent }, { path: '', pathMatch: 'full', redirectTo: 'dashboard' }
  ] },
  { path: 'client', component: ClientLayoutComponent, canActivate: [authGuard, roleGuard(['Client'])], children: [
    { path: 'dashboard', component: ClientDashboardComponent }, { path: 'credits/simulator', component: CreditSimulatorComponent }, { path: 'credits/comparison', component: CreditComparisonComponent }, { path: 'investments/simulator', component: InvestmentSimulatorComponent }, { path: 'investments/applications/:id', component: InvestmentApplicationComponent }, { path: '', pathMatch: 'full', redirectTo: 'dashboard' }
  ] },
  { path: 'advisor', canActivate: [authGuard, roleGuard(['Advisor'])], children: [{ path: 'dashboard', component: AdvisorDashboardComponent }, { path: '', pathMatch: 'full', redirectTo: 'dashboard' }] },
  { path: '', pathMatch: 'full', redirectTo: 'login' }, { path: '**', redirectTo: 'login' }
];
